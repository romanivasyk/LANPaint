using LANPaint.Dialogs.Service;
using LANPaint.Extensions;
using LANPaint.Model;
using LANPaint.Services.Network;
using LANPaint.Services.UDP;
using LANPaint.Services.UDP.Factory;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Ink;
using System.Windows.Media;

namespace LANPaint.ViewModels
{
    public class PaintViewModel : BindableBase, IDisposable
    {
        private bool _isEraser;
        private Color _backgroundColor;
        private bool _isReceive;
        private bool _isBroadcast;
        private StrokeCollection _strokes;

        public bool IsEraser
        {
            get => _isEraser;
            set => SetProperty(ref _isEraser, value);
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
        public RelayCommand OpenSettingsCommand { get; }
        public RelayCommand UndoCommand { get; }

        private readonly IDialogService _dialogService;
        private readonly IUDPBroadcastFactory _udpBroadcastFactory;
        private readonly ConcurrentBag<Stroke> _receivedStrokes;

        private IUDPBroadcast _udpBroadcastService;
        private CancellationTokenSource _cancelReceiveTokenSource;

        public PaintViewModel(IUDPBroadcastFactory udpBroadcastFactory, IDialogService dialogService)
        {
            _udpBroadcastFactory = udpBroadcastFactory;
            var netHelper = new NetworkInterfaceHelper();
            var localInterfaceAddressToConnect =
                netHelper.GetIpAddress(netHelper.GetIPv4Interfaces().First(ni => netHelper.IsReadyToUse(ni)));
            _udpBroadcastService = _udpBroadcastFactory.Create(localInterfaceAddressToConnect);
            _dialogService = dialogService;
            _receivedStrokes = new ConcurrentBag<Stroke>();
            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;

            ClearCommand = new RelayCommand(ClearCommandHandler, () => Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand(() => IsEraser = false, () => IsEraser);
            ChooseEraserCommand = new RelayCommand(() => IsEraser = true, () => !IsEraser);
            SaveDrawingCommand = new RelayCommand(OnSaveDrawing);
            OpenDrawingCommand = new RelayCommand(OnOpenDrawing);
            BroadcastChangedCommand = new RelayCommand(OnBroadcastChanged);
            ReceiveChangedCommand = new RelayCommand(OnReceiveChanged);
            OpenSettingsCommand = new RelayCommand(OnOpenSettings);
            UndoCommand = new RelayCommand(OnUndo);
            PropertyChanged += PropertyChangedHandler;
        }

        private async void ClearCommandHandler()
        {
            Strokes.Clear();
            _receivedStrokes.Clear();

            if (!IsBroadcast) return;
            var info = new DrawingInfo(ARGBColor.Default, SerializableStroke.Default, IsEraser, true);
            var serializer = new BinaryFormatter();
            var bytes = serializer.OneLineSerialize(info);
            await _udpBroadcastService.SendAsync(bytes);
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
                    exception?.InnerException is ObjectDisposedException disposedException &&
                    (disposedException.ObjectName == typeof(Socket).FullName ||
                     disposedException.ObjectName == typeof(UdpClient).FullName))
                { }
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
            using var settingsVm = new SettingsViewModel(_udpBroadcastService.LocalEndPoint.Address,
                _udpBroadcastService.LocalEndPoint.Port);

            var ipAddress = _dialogService.OpenDialog(settingsVm);

            if ((_udpBroadcastService.LocalEndPoint.Address.Equals(ipAddress) || ipAddress.Equals(IPAddress.None)) &&
                (_udpBroadcastService.LocalEndPoint.Port == settingsVm.Port || settingsVm.Port == default))
                return;

            _udpBroadcastService = _udpBroadcastFactory.Create(ipAddress, settingsVm.Port);
            IsBroadcast = IsReceive = false;
        }

        private void OnUndo()
        {
            if(Strokes.Count<1) return;
            Strokes.Remove(Strokes[^1]);
        }

        private async void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsBroadcast || e.PropertyName != nameof(Background)) return;
            var info = new DrawingInfo(ARGBColor.FromColor(Background), SerializableStroke.Default, IsEraser);
            var serializer = new BinaryFormatter();
            var bytes = serializer.OneLineSerialize(info);
            await _udpBroadcastService.SendAsync(bytes);
        }

        private async Task Receive(CancellationToken token)
        {
            await _udpBroadcastService.ClearBufferAsync();
            while (true)
            {
                var data = await _udpBroadcastService.ReceiveAsync(token);

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
            if (e.Added.Count <= 0 || !IsBroadcast) return;
            var strokesToSend = e.Added.Where(addedStroke => !_receivedStrokes.Contains(addedStroke)).ToArray();
            if (strokesToSend.Length < 1) return;

            await Task.Run(async () =>
            {
                foreach (var stroke in strokesToSend)
                {
                    var serializableStroke = SerializableStroke.FromStroke(stroke);
                    var info = new DrawingInfo(Background, serializableStroke, IsEraser);
                    var serializer = new BinaryFormatter();
                    var bytes = serializer.OneLineSerialize(info);
                    await _udpBroadcastService.SendAsync(bytes);
                }
            });
        }

        public void Dispose() => _udpBroadcastService?.Dispose();
    }
}
