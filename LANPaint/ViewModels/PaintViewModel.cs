using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;
using LANPaint.Dialogs.Service;
using LANPaint.Extensions;
using LANPaint.Model;
using LANPaint.MVVM;
using LANPaint.Services.Broadcast;
using LANPaint.Services.IO;
using LANPaint.Services.Network;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace LANPaint.ViewModels
{
    public class PaintViewModel : BindableBase, IDisposable, IDrawingStateVisitor
    {
        private bool _isEraser;
        private Color _backgroundColor;
        private bool _isReceive;
        private bool _isBroadcast;
        private StrokeCollection _strokes;

#warning Change IsBroadcast and IsEraser property to Tool enum?
#warning Or even by some ITool interface, each impl of which will have own behavior strategy?
        public bool IsEraser
        {
            get => _isEraser;
            set
            {
                if (!SetProperty(ref _isEraser, value)) return;
                ChooseEraserCommand?.RaiseCanExecuteChanged();
                ChoosePenCommand?.RaiseCanExecuteChanged();
            }
        }
        public Color Background
        {
            get => _backgroundColor;
            set => SetProperty(ref _backgroundColor, value);
        }
        public bool IsReceive
        {
            get => _isReceive;
            set => SetProperty(ref _isReceive, value);
        }
        public bool IsBroadcast
        {
            get => _isBroadcast;
            set => SetProperty(ref _isBroadcast, value);
        }
        public StrokeCollection Strokes
        {
            get => _strokes;
            set => SetProperty(ref _strokes, value);
        }

        public RelayCommand ClearCommand { get; }
        public RelayCommand ChoosePenCommand { get; }
        public RelayCommand ChooseEraserCommand { get; }
        public RelayCommand SaveDrawingCommand { get; }
        public RelayCommand OpenDrawingCommand { get; }
        public RelayCommand BroadcastChangedCommand { get; }
        public RelayCommand ReceiveChangedCommand { get; }
        public RelayCommand SynchronizeCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }

        private readonly IDialogService _dialogService;
        private readonly IBroadcastService _broadcastService;
        private readonly IFileService _fileService;
        private readonly List<Stroke> _receivedStrokes;
        private readonly Stack<(Stroke previous, Stroke undone)> _undoneStrokesStack;

        public PaintViewModel(IBroadcastService broadcastService, IDialogService dialogService, IFileService fileService)
        {
            _broadcastService = broadcastService ?? throw new ArgumentNullException(nameof(broadcastService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));

            var networkInterfaceHelper = NetworkInterfaceHelper.GetInstance();
            if (networkInterfaceHelper.IsAnyNetworkAvailable)
            {
                _broadcastService.Initialize(networkInterfaceHelper.GetAnyReadyToUseIPv4Address());
            }
            _broadcastService.DataReceived += OnDataReceived;
            _broadcastService.ConnectionLost += OnConnectionLost;

            _dialogService = dialogService;
            _receivedStrokes = new List<Stroke>();
            _undoneStrokesStack = new Stack<(Stroke previous, Stroke undone)>();
            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;

            ClearCommand = new RelayCommand(OnClear, () => Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand(() => IsEraser = false, () => IsEraser);
            ChooseEraserCommand = new RelayCommand(() => IsEraser = true, () => !IsEraser);
            SaveDrawingCommand = new RelayCommand(OnSaveDrawing, () => Strokes.Count > 0);
            OpenDrawingCommand = new RelayCommand(OnOpenDrawing);
            BroadcastChangedCommand = new RelayCommand(OnBroadcastChanged, () => _broadcastService.IsReady);
            ReceiveChangedCommand = new RelayCommand(OnReceiveChanged, () => _broadcastService.IsReady);
            SynchronizeCommand = new RelayCommand(OnSynchronize, () => _broadcastService.IsReady && Strokes.Count > 0);
            OpenSettingsCommand = new RelayCommand(OnOpenSettings);
            UndoCommand = new RelayCommand(OnUndo);
            RedoCommand = new RelayCommand(OnRedo);
            PropertyChanged += PropertyChangedHandler;
        }

        private void OnDataReceived(object sender, DataReceivedEventArgs e)
        {
            var binarySerializer = new BinaryFormatter();
            var deserializedInfo = binarySerializer.OneLineDeserialize(e.Data) as IDrawingComponent;

            deserializedInfo?.AcceptDrawingState(this);
        }

        private async void OnClear()
        {
            ClearState();

            if (!IsBroadcast) return;
            var info = new ClearDrawingStateComponent();
            await SendDataAsync(info);
        }

        private async void OnSaveDrawing()
        {
            var settings = new SaveFileDialogSettings
            {
                Title = "Save Snapshot...",
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Filter = "LANPaint Snapshots (*.lpsnp)|*.lpsnp",
                OverwritePrompt = true
            };

            var saveDialogResult = _dialogService.ShowSaveFileDialog(this, settings);
            if (saveDialogResult == false) return;

            var snapshot = TakeSnapshot();
            await _fileService.SaveToFileAsync(snapshot, settings.FileName);
        }

        private async void OnOpenDrawing()
        {
            if (Strokes.Count > 0)
            {
                var questionResult = _dialogService.ShowMessageBox(this, "Do you want to save current board?",
                    "LANPaint - Save current board", MessageBoxButton.YesNo, MessageBoxImage.Question,
                    MessageBoxResult.No);

                if (questionResult == MessageBoxResult.Yes) OnSaveDrawing();
            }

            var settings = new OpenFileDialogSettings
            {
                Title = "Open Snapshot...",
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Filter = "LANPaint Snapshots (*.lpsnp)|*.lpsnp"
            };

            var openDialogResult = _dialogService.ShowOpenFileDialog(this, settings);
            if (openDialogResult == false) return;

            var dataFromFile = await _fileService.ReadFromFileAsync(settings.FileName);
            if (dataFromFile is SnapshotStateComponent snapshot)
            {
                SnapshotState(snapshot);
            }
            else
            {
                _dialogService.ShowMessageBox(this, "File doesn't contain Snapshot or corrupted",
                    "Error while reading the file", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            }
        }

        private void OnBroadcastChanged()
        {
            if (IsBroadcast)
            {
                //TODO: Setup broadcast
            }
            else
            {
                //TODO: Raise cancellation
            }
        }

        private async void OnReceiveChanged()
        {
            if (IsReceive)
            {
                await _broadcastService.StartReceiveAsync();
            }
            else
            {
                _broadcastService.CancelReceive();
            }
        }

        private async void OnSynchronize()
        {
            var snapshot = TakeSnapshot();
            await SendDataAsync(snapshot);
        }

        private void OnOpenSettings()
        {
            var settingsVm = new SettingsViewModel(_broadcastService.LocalEndPoint.Address,
                _broadcastService.LocalEndPoint.Port);

            if (_dialogService.ShowCustomDialog(settingsVm) == false) return;

            if (Equals(settingsVm.Result, _broadcastService.LocalEndPoint)) return;

            _broadcastService.Initialize(settingsVm.Result.Address, settingsVm.Result.Port);

            BroadcastChangedCommand?.RaiseCanExecuteChanged();
            ReceiveChangedCommand?.RaiseCanExecuteChanged();
            SynchronizeCommand?.RaiseCanExecuteChanged();

            IsBroadcast = IsReceive = false;
        }

        private void OnUndo()
        {
            if (Strokes.Count < 1) return;
            var undoneItem = (Strokes.ElementAtOrDefault(Strokes.Count - 2), Strokes[^1]);
            _undoneStrokesStack.Push(undoneItem);
            Strokes.Remove(Strokes[^1]);
        }

        private void OnRedo()
        {
            if (_undoneStrokesStack.Count < 1) return;
            if (_undoneStrokesStack.Peek().previous != Strokes.ElementAtOrDefault(Strokes.Count - 1))
            {
                _undoneStrokesStack.Clear();
                return;
            }

            Strokes.Add(_undoneStrokesStack.Pop().undone);
        }

        private async void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsBroadcast || e.PropertyName != nameof(Background)) return;
            var info = new BackgroundStateComponent(Background);
            await SendDataAsync(info);
        }

        private async void OnStrokesCollectionChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            ClearCommand?.RaiseCanExecuteChanged();
            SaveDrawingCommand?.RaiseCanExecuteChanged();
            SynchronizeCommand?.RaiseCanExecuteChanged();

            if (e.Added.Count <= 0 || !IsBroadcast) return;
            var strokesToSend = e.Added.Where(addedStroke => !_receivedStrokes.Contains(addedStroke)).ToArray();
            if (strokesToSend.Length < 1) return;

            foreach (var stroke in strokesToSend)
            {
                var info = new DrawStateComponent(stroke);
                await SendDataAsync(info);
                if (!IsBroadcast) break;
            }
        }

        private async Task<int> SendDataAsync(object data)
        {
            var serializer = new BinaryFormatter();
            var bytes = serializer.OneLineSerialize(data);
            return await _broadcastService.SendAsync(bytes);
        }

        private void OnConnectionLost()
        {
            BroadcastChangedCommand.RaiseCanExecuteChanged();
            ReceiveChangedCommand.RaiseCanExecuteChanged();
            SynchronizeCommand.RaiseCanExecuteChanged();

            if (!IsBroadcast && !IsReceive) return;
            IsBroadcast = IsReceive = false;
            _dialogService.ShowMessageBox(this,
                "The PC was unexpectedly disconnected from the network. Please, go to Settings to setup new connection.",
                "LANPaint - Connection Lost", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
        }

        private SnapshotStateComponent TakeSnapshot()
        {
            var strokes = Strokes.Select(SerializableStroke.FromStroke).ToList();
            return new SnapshotStateComponent(ARGBColor.FromColor(Background), strokes);
        }

        //private void ApplySnapshot(BoardSnapshot snapshot)
        //{
        //    ClearBoard();
        //    Background = snapshot.Background.AsColor();
        //    snapshot.Strokes.Select(stroke => stroke.ToStroke()).ToList().ForEach(stroke => Strokes.Add(stroke));
        //}

        //private void ClearBoard()
        //{
        //    Strokes.Clear();
        //    _receivedStrokes.Clear();
        //}

        public void Dispose()
        {

            _broadcastService.DataReceived -= OnDataReceived;
            _broadcastService.ConnectionLost -= OnConnectionLost;
            _broadcastService?.Dispose();
        }

        public void ClearState()
        {
            Strokes.Clear();
            _receivedStrokes.Clear();
        }

        public void ChangeBackgroundState(BackgroundStateComponent backgroundComponent)
        {
            Background = backgroundComponent.Background.AsColor();
        }

        public void EraseState(EraseStateComponent eraseComponent)
        {
            var stroke = eraseComponent.Stroke.ToStroke();
            stroke.DrawingAttributes.Color = Background;

            _receivedStrokes.Add(stroke);
            Strokes.Add(stroke);
        }

        public void DrawState(DrawStateComponent drawComponent)
        {
            var stroke = drawComponent.Stroke.ToStroke();
            _receivedStrokes.Add(stroke);
            Strokes.Add(stroke);
        }

        public void SnapshotState(SnapshotStateComponent snapshotComponent)
        {
            ClearState();
            Background = snapshotComponent.Background.AsColor();
            snapshotComponent.Strokes.Select(stroke => stroke.ToStroke()).ToList().ForEach(stroke => Strokes.Add(stroke));
        }
    }
}
