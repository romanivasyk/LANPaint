using LANPaint.Dialogs.Service;
using LANPaint.Extensions;
using LANPaint.Model;
using LANPaint.Services.Network;
using LANPaint.Services.UDP;
using LANPaint.Services.UDP.Factory;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using LANPaint.Dialogs.Alerts;

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
            }
        }

        public RelayCommand ClearCommand { get; }
        public RelayCommand ChoosePenCommand { get; }
        public RelayCommand ChooseEraserCommand { get; }
        public RelayCommand SaveDrawingCommand { get; }
        public RelayCommand OpenDrawingCommand { get; }
        public RelayCommand BroadcastChangedCommand { get; }
        public RelayCommand ReceiveChangedCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }

        private readonly IDialogService _dialogService;
        private readonly IUDPBroadcastFactory _udpBroadcastFactory;
        private readonly ConcurrentBag<Stroke> _receivedStrokes;
        private readonly Stack<(Stroke previous, Stroke undone)> _undoneStrokesStack;
        private CancellationTokenSource _cancelReceiveTokenSource;
        private Dispatcher _dispatcher;

        public PaintViewModel(IUDPBroadcastFactory udpBroadcastFactory, IDialogService dialogService)
        {
            _dispatcher = Dispatcher.CurrentDispatcher;
            _udpBroadcastFactory = udpBroadcastFactory;
            var netHelper = NetworkInterfaceHelper.GetInstance();
            if (netHelper.IsAnyNetworkAvailable)
                UdpBroadcastService = _udpBroadcastFactory.Create(netHelper.GetAnyReadyToUseIPv4Address());

            _dialogService = dialogService;
            _receivedStrokes = new ConcurrentBag<Stroke>();
            _undoneStrokesStack = new Stack<(Stroke previous, Stroke undone)>();
            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;

            ClearCommand = new RelayCommand(OnClear, () => Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand(() => IsEraser = false, () => IsEraser);
            ChooseEraserCommand = new RelayCommand(() => IsEraser = true, () => !IsEraser);
            SaveDrawingCommand = new RelayCommand(OnSaveDrawing);
            OpenDrawingCommand = new RelayCommand(OnOpenDrawing);
            BroadcastChangedCommand = new RelayCommand(OnBroadcastChanged, () => UdpBroadcastService != null);
            ReceiveChangedCommand = new RelayCommand(OnReceiveChanged, () => UdpBroadcastService != null);
            OpenSettingsCommand = new RelayCommand(OnOpenSettings);
            UndoCommand = new RelayCommand(OnUndo);
            RedoCommand = new RelayCommand(OnRedo);
            PropertyChanged += PropertyChangedHandler;
        }

        private async void OnClear()
        {
            Strokes.Clear();
            _receivedStrokes.Clear();

            if (!IsBroadcast) return;
            var info = new DrawingInfo(ARGBColor.Default, SerializableStroke.Default, IsEraser, true);
            await SendDrawingInfo(info);
        }

        private void OnSaveDrawing()
        {
            throw new NotImplementedException();
            //TODO: Save all drawing data into object, serialize it and save to file
        }

        private void OnOpenDrawing()
        {
            throw new NotImplementedException();
            //TODO: Add suggestion to save current work in case board not empty???
            //TODO: Read file, deserialize to object and apply to current border
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

        private void OnOpenSettings()
        {
            SettingsViewModel settingsVm;

            if (UdpBroadcastService != null)
                settingsVm = new SettingsViewModel(UdpBroadcastService.LocalEndPoint.Address,
                    UdpBroadcastService.LocalEndPoint.Port);
            else
                settingsVm = new SettingsViewModel();

            var resultEndPoint = _dialogService.OpenDialog(settingsVm);

            if ((UdpBroadcastService != null && Equals(resultEndPoint, UdpBroadcastService.LocalEndPoint)) ||
                resultEndPoint == null) return;

            UdpBroadcastService = _udpBroadcastFactory.Create(resultEndPoint.Address, resultEndPoint.Port);
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
            if(_undoneStrokesStack.Count < 1) return;
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
            await SendDrawingInfo(info);
        }

        private async Task Receive(CancellationToken token)
        {
            await UdpBroadcastService.ClearBufferAsync();
            while (true)
            {
                var data = await UdpBroadcastService.ReceiveAsync(token);

                if (data == null || data.Length == 0)
                    continue;

                var info = await Task.Run(() =>
                {
                    var binarySerializer = new BinaryFormatter();
                    return binarySerializer.OneLineDeserialize<DrawingInfo>(data);
                });


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
        }

        private async void OnStrokesCollectionChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            ClearCommand?.RaiseCanExecuteChanged();
            if (e.Added.Count <= 0 || !IsBroadcast) return;
            var strokesToSend = e.Added.Where(addedStroke => !_receivedStrokes.Contains(addedStroke)).ToArray();
            if (strokesToSend.Length < 1) return;

            foreach (var stroke in strokesToSend)
            {
                var info = new DrawingInfo(Background, SerializableStroke.FromStroke(stroke), IsEraser);
                await SendDrawingInfo(info);
                if (!IsBroadcast) break;
            }
        }

        private async Task<int> SendDrawingInfo(DrawingInfo info)
        {
            var serializer = new BinaryFormatter();
            var bytes = serializer.OneLineSerialize(info);
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
            UdpBroadcastService.Dispose();
            UdpBroadcastService = null;
            IsBroadcast = IsReceive = false;

            var alertDialogService = new DialogService();
            var alertVm = new AlertDialogViewModel("Connection Lost", "The PC was unexpectedly disconnected from " +
                                                                      "the network. Please, go to Settings to setup new connection.");
            alertDialogService.OpenDialog(alertVm);

        }

        public void Dispose() => UdpBroadcastService?.Dispose();
    }
}
