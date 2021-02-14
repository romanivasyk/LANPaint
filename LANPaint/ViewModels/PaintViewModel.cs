using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;
using LANPaint.Dialogs.Service;
using LANPaint.Extensions;
using LANPaint.Model;
using LANPaint.Services.Network;
using LANPaint.Services.UDP;
using LANPaint.Services.UDP.Factory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;

namespace LANPaint.ViewModels
{
    public class PaintViewModel : BindableBase, IDisposable
    {
        private bool _isEraser;
        private Color _backgroundColor;
        private bool _isReceive;
        private bool _isBroadcast;
        private StrokeCollection _strokes;
        private IUDPBroadcast _udpBroadcastService;

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
        public IUDPBroadcast UdpBroadcastService
        {
            get => _udpBroadcastService;
            private set
            {
                SetProperty(ref _udpBroadcastService, value);
                BroadcastChangedCommand?.RaiseCanExecuteChanged();
                ReceiveChangedCommand?.RaiseCanExecuteChanged();
                SynchronizeCommand?.RaiseCanExecuteChanged();
            }
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
        private readonly IUDPBroadcastFactory _udpBroadcastFactory;
        private readonly ConcurrentBag<Stroke> _receivedStrokes;
        private readonly Stack<(Stroke previous, Stroke undone)> _undoneStrokesStack;
        private readonly NetworkInterfaceHelper _netHelper;
        private readonly Dispatcher _dispatcher;
        private CancellationTokenSource _cancelReceiveTokenSource;

        public PaintViewModel(IUDPBroadcastFactory udpBroadcastFactory, IDialogService dialogService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _udpBroadcastFactory = udpBroadcastFactory;
            _netHelper = NetworkInterfaceHelper.GetInstance();
            _netHelper.Interfaces.CollectionChanged += NetworkInterfacesCollectionChangedHandler;
            if (_netHelper.IsAnyNetworkAvailable)
                UdpBroadcastService = _udpBroadcastFactory.Create(_netHelper.GetAnyReadyToUseIPv4Address());

            _dialogService = dialogService;
            _receivedStrokes = new ConcurrentBag<Stroke>();
            _undoneStrokesStack = new Stack<(Stroke previous, Stroke undone)>();
            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;

            ClearCommand = new RelayCommand(OnClear, () => Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand(() => IsEraser = false, () => IsEraser);
            ChooseEraserCommand = new RelayCommand(() => IsEraser = true, () => !IsEraser);
            SaveDrawingCommand = new RelayCommand(OnSaveDrawing, () => Strokes.Count > 0);
            OpenDrawingCommand = new RelayCommand(OnOpenDrawing);
            BroadcastChangedCommand = new RelayCommand(OnBroadcastChanged, () => UdpBroadcastService != null);
            ReceiveChangedCommand = new RelayCommand(OnReceiveChanged, () => UdpBroadcastService != null);
            SynchronizeCommand = new RelayCommand(OnSynchronize, () => UdpBroadcastService != null && Strokes.Count > 0);
            OpenSettingsCommand = new RelayCommand(OnOpenSettings);
            UndoCommand = new RelayCommand(OnUndo);
            RedoCommand = new RelayCommand(OnRedo);
            PropertyChanged += PropertyChangedHandler;
        }

        private void NetworkInterfacesCollectionChangedHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (UdpBroadcastService == null || _netHelper.IsReadyToUse(UdpBroadcastService.LocalEndPoint.Address)) return;
            _dispatcher.Invoke(() =>
            {
                UdpBroadcastService?.Dispose();
                UdpBroadcastService = null;

                if (IsBroadcast || IsReceive)
                {
                    ShowAlert("LANPaint - Connection Lost", "The PC was unexpectedly disconnected from " +
                                                 "the network. Please, go to Settings to setup new connection.");
                }
                IsBroadcast = IsReceive = false;
            });
        }

        private async void OnClear()
        {
            Strokes.Clear();
            _receivedStrokes.Clear();

            if (!IsBroadcast) return;
            var info = new DrawingInfo(ARGBColor.Default, SerializableStroke.Default, IsEraser, true);
            await SendDataAsync(info);
        }

#warning REFACTOR THIS!
        private async void OnSaveDrawing()
        {
            var settings = new SaveFileDialogSettings
            {
                Title = "Save Snapshot...",
                InitialDirectory = System.IO.Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                Filter = "LANPaint Snapshots (*.lpsnp)|*.lpsnp"
            };

            var saveDialogResult = _dialogService.ShowSaveFileDialog(this, settings);
            if (saveDialogResult == false) return;

            var strokes = Strokes.Select(SerializableStroke.FromStroke).ToList();
            var snapshot = new BoardSnapshot(ARGBColor.FromColor(Background), strokes);
            var formatter = new BinaryFormatter();
            var bytes = formatter.OneLineSerialize(snapshot);

            await using var stream = new FileStream(settings.FileName, FileMode.Create, FileAccess.Write, FileShare.None);
            await stream.WriteAsync(bytes);
        }

#warning REFACTOR THIS!
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

            await using var stream = new FileStream(settings.FileName, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer);

            var formatter = new BinaryFormatter();
            var snapshot = (BoardSnapshot)formatter.OneLineDeserialize(buffer);

            Strokes.Clear();
            _receivedStrokes.Clear();

            Background = snapshot.Background.AsColor();
            snapshot.Strokes.Select(stroke => stroke.ToStroke()).ToList().ForEach(stroke => Strokes.Add(stroke));
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
                _cancelReceiveTokenSource = new CancellationTokenSource();
                try
                {
                    await Receive(_cancelReceiveTokenSource.Token);
                }
                catch (OperationCanceledException)
                { }
                catch (AggregateException exception) when (
                    exception.InnerException is ObjectDisposedException disposedException &&
                    (disposedException.ObjectName == typeof(Socket).FullName ||
                     disposedException.ObjectName == typeof(UdpClient).FullName))
                { }
                catch (SocketException exception)
                {
                    HandleBroadcasterSocketException(exception);
                }
                finally
                {
                    _cancelReceiveTokenSource?.Dispose();
                }
            }
            else
            {
                _cancelReceiveTokenSource.Cancel();
            }
        }

        private async void OnSynchronize()
        {
            var strokes = Strokes.Select(SerializableStroke.FromStroke).ToList();
            var snapshot = new BoardSnapshot(ARGBColor.FromColor(Background), strokes);
            await SendDataAsync(snapshot);
        }

        private void OnOpenSettings()
        {
            SettingsViewModel settingsVm;

            if (UdpBroadcastService != null)
                settingsVm = new SettingsViewModel(UdpBroadcastService.LocalEndPoint.Address,
                    UdpBroadcastService.LocalEndPoint.Port);
            else
                settingsVm = new SettingsViewModel();

            if (!_dialogService.ShowCustomDialog(settingsVm)) return;

            if ((UdpBroadcastService != null && Equals(settingsVm.Result, UdpBroadcastService.LocalEndPoint)) ||
                settingsVm.Result == null) return;

            UdpBroadcastService = _udpBroadcastFactory.Create(settingsVm.Result.Address, settingsVm.Result.Port);
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
            var info = new DrawingInfo(ARGBColor.FromColor(Background), SerializableStroke.Default, IsEraser);
            await SendDataAsync(info);
        }

        private async Task Receive(CancellationToken token)
        {
            await UdpBroadcastService.ClearBufferAsync();
            while (true)
            {
                var data = await UdpBroadcastService.ReceiveAsync(token);

                if (data == null || data.Length == 0)
                    continue;

                var deserializedInfo = await Task.Run(() =>
                {
                    var binarySerializer = new BinaryFormatter();
                    return binarySerializer.OneLineDeserialize(data);
                });

#warning REFACTOR this huge if/else type checking statement. (HOW?)
                if (deserializedInfo is DrawingInfo info)
                {

                    if (info.ClearBoard)
                    {
                        Strokes.Clear();
                        _receivedStrokes.Clear();
                        continue;
                    }

                    if (info.Background != ARGBColor.FromColor(Background) && info.Stroke == SerializableStroke.Default)
                    {
                        Background = info.Background.AsColor();
                    }

                    if (info.Stroke == SerializableStroke.Default) continue;
                    var stroke = info.Stroke.ToStroke();

                    if (info.IsEraser)
                    {
                        stroke.DrawingAttributes.Color = Background;
                    }

                    _receivedStrokes.Add(stroke);
                    Strokes.Add(stroke);
                }
                else
                {
                    var snapshot = (BoardSnapshot)deserializedInfo;

                    Strokes.Clear();
                    _receivedStrokes.Clear();

                    Background = snapshot.Background.AsColor();
                    snapshot.Strokes.Select(stroke => stroke.ToStroke()).ToList().ForEach(stroke => Strokes.Add(stroke));
                }
            }
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
                var info = new DrawingInfo(Background, SerializableStroke.FromStroke(stroke), IsEraser);
                await SendDataAsync(info);
                if (!IsBroadcast) break;
            }
        }

        private async Task<int> SendDataAsync(object data)
        {
            var serializer = new BinaryFormatter();
            var bytes = serializer.OneLineSerialize(data);
            int sendedBytesAmount = default;
            try
            {
                sendedBytesAmount = await UdpBroadcastService.SendAsync(bytes);
            }
            catch (SocketException exception)
            {
                HandleBroadcasterSocketException(exception);
            }

            return sendedBytesAmount;
        }

        private void HandleBroadcasterSocketException(SocketException exception)
        {
            Debug.WriteLine(exception.Message);
            Debug.WriteLine($"SocketErrorCode: {exception.SocketErrorCode}");
            UdpBroadcastService?.Dispose();
            UdpBroadcastService = null;
            IsBroadcast = IsReceive = false;

            ShowAlert("LANPaint - Connection Lost", "The PC was unexpectedly disconnected from " +
                                         "the network. Please, go to Settings to setup new connection.");
        }

        private void ShowAlert(string title, string message)
        {
            _dialogService.ShowMessageBox(this, message, title, MessageBoxButton.OK, MessageBoxImage.Error,
                MessageBoxResult.OK);
        }

        public void Dispose() => UdpBroadcastService?.Dispose();
    }
}
