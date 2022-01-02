using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using LANPaint.Dialogs.FrameworkDialogs.OpenFile;
using LANPaint.Dialogs.FrameworkDialogs.SaveFile;
using LANPaint.Dialogs.Service;
using LANPaint.DrawingInstructions;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Extensions;
using LANPaint.Model;
using LANPaint.MVVM;
using LANPaint.Services.Broadcast;
using LANPaint.Services.IO;
using LANPaint.Services.Network;

namespace LANPaint.ViewModels;

public class PaintViewModel : BindableBase, IDrawingInstructionRepository, IDisposable
{
    private bool _isEraser;
    private Color _backgroundColor;
    private bool _isReceive;
    private bool _isBroadcast;
    private StrokeCollection _strokes;

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
    private readonly INetworkServiceFactory _networkServiceFactory;
    private readonly List<Stroke> _receivedStrokes;
    private readonly Stack<(Stroke previous, Stroke undone)> _undoneStrokesStack;
    private readonly Color _defaultBackground = Colors.White;
    private Color? _receivedBackground;

    public PaintViewModel(IBroadcastService broadcastService, IDialogService dialogService, IFileService fileService,
        INetworkServiceFactory networkServiceFactory)
    {
        _broadcastService = broadcastService ?? throw new ArgumentNullException(nameof(broadcastService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _networkServiceFactory = networkServiceFactory ?? throw new ArgumentNullException(nameof(networkServiceFactory));

        _broadcastService.DataReceived += OnDataReceived;
        _broadcastService.ConnectionLost += OnConnectionLost;

        _dialogService = dialogService;
        _receivedStrokes = new List<Stroke>();
        _undoneStrokesStack = new Stack<(Stroke previous, Stroke undone)>();
        Strokes = new StrokeCollection();
        Strokes.StrokesChanged += OnStrokesCollectionChanged;
        Background = _defaultBackground;

        ClearCommand = new RelayCommand(OnClear, () => Strokes.Count > 0 || Background != _defaultBackground);
        ChoosePenCommand = new RelayCommand(() => IsEraser = false, () => IsEraser);
        ChooseEraserCommand = new RelayCommand(() => IsEraser = true, () => !IsEraser);
        SaveDrawingCommand = new RelayCommand(OnSaveDrawing, () => Strokes.Count > 0 || Background != _defaultBackground);
        OpenDrawingCommand = new RelayCommand(OnOpenDrawing);
        BroadcastChangedCommand = new RelayCommand(OnBroadcastChanged, () => _broadcastService.IsReady);
        ReceiveChangedCommand = new RelayCommand(OnReceiveChanged, () => _broadcastService.IsReady);
        SynchronizeCommand = new RelayCommand(OnSynchronize,
            () => _broadcastService.IsReady && (Strokes.Count > 0 || Background != _defaultBackground));
        OpenSettingsCommand = new RelayCommand(OnOpenSettings);
        UndoCommand = new RelayCommand(OnUndo);
        RedoCommand = new RelayCommand(OnRedo);
        PropertyChanged += PropertyChangedHandler;
    }

    private void OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        var binarySerializer = new BinaryFormatter();
        var deserializedInfo = binarySerializer.OneLineDeserialize(e.Data) as IDrawingInstruction;

        deserializedInfo?.ExecuteDrawingInstruction(this);
    }

    private async void OnClear()
    {
        Clear();

        if (!IsBroadcast) return;
        var info = new ClearInstruction();
        await SendDataAsync(info);
    }

    private async void OnSaveDrawing()
    {
        var settings = new SaveFileDialogSettings
        {
            Title = "Save Snapshot...",
            InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
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
            InitialDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
            Filter = "LANPaint Snapshots (*.lpsnp)|*.lpsnp"
        };

        var openDialogResult = _dialogService.ShowOpenFileDialog(this, settings);
        if (openDialogResult == false) return;

        await ApplyFromFile(settings.FileName);
    }

    public async Task ApplyFromFile(string fileName)
    {
        object dataFromFile;
        try
        {
            dataFromFile = await _fileService.ReadFromFileAsync(fileName);
        }
        catch (ArgumentException)
        {
            _dialogService.ShowMessageBox(this, "LANPaint cannot open this file.",
                "Error while reading the file", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
            return;
        }


        if (dataFromFile is SnapshotInstruction snapshot) ApplySnapshot(snapshot);
        else
            _dialogService.ShowMessageBox(this, "File doesn't contain Snapshot or corrupted",
                "Error while reading the file", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
    }

    private void OnBroadcastChanged()
    {
        if (IsBroadcast)
        {
            //Setup broadcast
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
        using var watcher = _networkServiceFactory.CreateWatcher();
        var settingsVm = new SettingsViewModel(watcher, _networkServiceFactory.CreateUtility(),
            _broadcastService.LocalEndPoint.Address, _broadcastService.LocalEndPoint.Port);

        if (_dialogService.ShowCustomDialog(settingsVm) == false) return;

        if (Equals(settingsVm.Result, _broadcastService.LocalEndPoint)) return;

        _broadcastService.Initialize(settingsVm.Result.Address, settingsVm.Result.Port);

        RaiseNetworkRelatedCanExecuteChanged();
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
        if (e.PropertyName == nameof(Background))
        {
            RaiseStrokeRelatedCanExecuteChanged();
            if (!IsBroadcast || _receivedBackground == Background) return;
            var info = new ChangeBackgroundInstruction(Background);
            await SendDataAsync(info);
        }
    }

    private async void OnStrokesCollectionChanged(object sender, StrokeCollectionChangedEventArgs e)
    {
        RaiseStrokeRelatedCanExecuteChanged();

        if (e.Added.Count <= 0 || !IsBroadcast) return;
        var strokesToSend = e.Added.Where(addedStroke => !_receivedStrokes.Contains(addedStroke)).ToArray();
        if (strokesToSend.Length < 1) return;

        foreach (var stroke in strokesToSend)
        {
            IDrawingInstruction instruction;
            if (IsEraser) instruction = new EraseInstruction(stroke);
            else instruction = new DrawInstruction(stroke);

            await SendDataAsync(instruction);
            if (!IsBroadcast) break;
        }
    }

    private async Task<int> SendDataAsync(object data)
    {
        var serializer = new BinaryFormatter();
        var bytes = serializer.OneLineSerialize(data);
        return await _broadcastService.SendAsync(bytes);
    }

    private void OnConnectionLost(object sender, EventArgs e)
    {
        RaiseNetworkRelatedCanExecuteChanged();

        if (!IsBroadcast && !IsReceive) return;
        IsBroadcast = IsReceive = false;
        _dialogService.ShowMessageBox(this,
            "The PC was unexpectedly disconnected from the network. Please, go to Settings to setup new connection.",
            "LANPaint - Connection Lost", MessageBoxButton.OK, MessageBoxImage.Error, MessageBoxResult.OK);
    }

    private SnapshotInstruction TakeSnapshot()
    {
        var strokes = Strokes.Select(SerializableStroke.FromStroke).ToList();
        return new SnapshotInstruction(ARGBColor.FromColor(Background), strokes);
    }

    public void Clear()
    {
        Strokes.Clear();
        _receivedStrokes.Clear();
        Background = _defaultBackground;
        RaiseStrokeRelatedCanExecuteChanged();
    }

    public void ChangeBackground(ChangeBackgroundInstruction changeBackgroundInstruction)
    {
        _receivedBackground = changeBackgroundInstruction.Background.AsColor();
        Background = changeBackgroundInstruction.Background.AsColor();
        RaiseStrokeRelatedCanExecuteChanged();
    }

    public void Erase(EraseInstruction instruction)
    {
        var stroke = instruction.ErasingStroke.ToStroke();
        stroke.DrawingAttributes.Color = Background;

        _receivedStrokes.Add(stroke);
        Strokes.Add(stroke);
    }

    public void Draw(DrawInstruction instruction)
    {
        var stroke = instruction.DrawingStroke.ToStroke();
        _receivedStrokes.Add(stroke);
        Strokes.Add(stroke);
    }

    public void ApplySnapshot(SnapshotInstruction instruction)
    {
        Clear();
        Background = instruction.Background.AsColor();
        instruction.Strokes.Select(stroke => stroke.ToStroke()).ToList().ForEach(stroke => Strokes.Add(stroke));
        RaiseStrokeRelatedCanExecuteChanged();
    }

    private void RaiseStrokeRelatedCanExecuteChanged()
    {
        ClearCommand?.RaiseCanExecuteChanged();
        SaveDrawingCommand?.RaiseCanExecuteChanged();
        SynchronizeCommand?.RaiseCanExecuteChanged();
    }

    private void RaiseNetworkRelatedCanExecuteChanged()
    {
        BroadcastChangedCommand.RaiseCanExecuteChanged();
        ReceiveChangedCommand.RaiseCanExecuteChanged();
        SynchronizeCommand.RaiseCanExecuteChanged();
    }

    public void Dispose()
    {
        _broadcastService.DataReceived -= OnDataReceived;
        _broadcastService.ConnectionLost -= OnConnectionLost;
    }
}