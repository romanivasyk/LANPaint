using LANPaint.Extensions;
using LANPaint.Model;
using LANPaint.Services.UDP;
using LANPaint.Services.UDP.Factory;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using LANPaint.Dialogs;
using LANPaint.Dialogs.Service;

namespace LANPaint.ViewModels
{
    public class PaintViewModel : BindableBase, IDisposable
    {
        private bool _isEraser;
        private StrokeCollection _strokes;
        private Color _backgroundColor;
        private bool _isReceive;
        private bool _isBroadcast;

        public bool IsEraser
        {
            get => _isEraser;
            set => SetProperty(ref _isEraser, value);
        }
        public StrokeCollection Strokes
        {
            get => _strokes;
            set => SetProperty(ref _strokes, value);
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

        public RelayCommand ClearCommand { get; }
        public RelayCommand ChoosePenCommand { get; }
        public RelayCommand ChooseEraserCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand OpenCommand { get; }
        public RelayCommand BroadcastChangedCommand { get; }
        public RelayCommand ReceiveChangedCommand { get; }
        public RelayCommand OpenSettingsCommand { get; }

        private readonly IDialogService _dialogService;
        private readonly IUDPBroadcastFactory _broadcastFactory;
        private readonly ConcurrentBag<Stroke> _receivedStrokes;

        private IUDPBroadcast _broadcastService;
        private CancellationTokenSource _receiveTokenSource;

        public PaintViewModel(IUDPBroadcastFactory udpBroadcastFactory, IDialogService dialogService)
        {
            _broadcastFactory = udpBroadcastFactory;
            _broadcastService = _broadcastFactory.Create(IPAddress.Parse(((App)Application.Current).Args[0]));
            _dialogService = dialogService;
            _receivedStrokes = new ConcurrentBag<Stroke>();

            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;

            ClearCommand = new RelayCommand(ClearCommandHandler,  ()=> Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand( ()=> IsEraser = false,  ()=> IsEraser);
            ChooseEraserCommand = new RelayCommand(() => IsEraser = true, () => !IsEraser);
            SaveCommand = new RelayCommand(OnSaveExecuted);
            OpenCommand = new RelayCommand(OnOpenExecuted);
            BroadcastChangedCommand = new RelayCommand(OnBroadcastChanged);
            ReceiveChangedCommand = new RelayCommand(OnReceiveChanged);
            OpenSettingsCommand = new RelayCommand(OnOpenSettings);
            PropertyChanged += PropertyChangedHandler;
        }

        private void OnOpenSettings()
        {
            using var settingsVm = new SettingsViewModel();
            var settings = _dialogService.OpenDialog(settingsVm);

            if ((_broadcastService.LocalEndPoint.Port == settings.Port || settings.Port == default) &&
                (_broadcastService.LocalEndPoint.Address.Equals(settings.IpAddress) || settings.IpAddress.Equals(IPAddress.None)))
                return;

            var cachedReceive = IsReceive;
            var cachedBroadcast = IsBroadcast;

            IsReceive = IsBroadcast = false;
            _broadcastService.Dispose();
            _broadcastService = _broadcastFactory.Create(settings.IpAddress, settings.Port);
            IsReceive = cachedReceive;
            IsBroadcast = cachedBroadcast;
        }

        private async void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (!IsBroadcast || e.PropertyName != nameof(Background)) return;
            var info = new DrawingInfo(ARGBColor.FromColor(Background), SerializableStroke.Default, IsEraser);
            var serializer = new BinaryFormatter();
            var bytes = serializer.OneLineSerialize(info);
            await _broadcastService.SendAsync(bytes);
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
                _receiveTokenSource = new CancellationTokenSource();
                try
                {
                    await Receive(_receiveTokenSource.Token);
                }
                catch (OperationCanceledException)
                { }
                finally
                {
                    _receiveTokenSource?.Dispose();
                }
            }
            else
            {
                _receiveTokenSource.Cancel();
            }
        }

        private async void ClearCommandHandler()
        {
            Strokes.Clear();
            _receivedStrokes.Clear();

            if (!IsBroadcast) return;
            var info = new DrawingInfo(ARGBColor.Default, SerializableStroke.Default, IsEraser, true);
            var serializer = new BinaryFormatter();
            var bytes = serializer.OneLineSerialize(info);
            await _broadcastService.SendAsync(bytes);
        }

        private void OnSaveExecuted()
        {
            throw new NotImplementedException();
            //TODO: Save all drawing data into object, serialize it and save to file
        }

        private void OnOpenExecuted()
        {
            throw new NotImplementedException();
            //TODO: Add suggestion to save current work in case board not empty???
            //TODO: Read file, deserialize to object and apply to current border
        }

        private async Task Receive(CancellationToken token)
        {
            await _broadcastService.ClearBufferAsync();
            while (true)
            {
                var data = await _broadcastService.ReceiveAsync(token);

                if (data == null || data.Length == 0)
                {
                    continue;
                }

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

                if (info.Stroke.Equals(SerializableStroke.Default)) continue;
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
            await Task.Run(async () =>
            {
                foreach (var stroke in e.Added)
                {
                    if (_receivedStrokes.Contains(stroke)) continue;
                    var serializableStroke = SerializableStroke.FromStroke(stroke);
                    var info = new DrawingInfo(Background, serializableStroke, IsEraser);
                    var serializer = new BinaryFormatter();
                    var bytes = serializer.OneLineSerialize(info);
                    await _broadcastService.SendAsync(bytes);
                }
            });
        }

        public void Dispose() => _broadcastService.Dispose();
    }
}
