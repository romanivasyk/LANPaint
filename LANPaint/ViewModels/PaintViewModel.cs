using LANPaint.Extensions;
using LANPaint.Model;
using LANPaint.DialogServices;
using LANPaint.Services.UDP;
using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

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

        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand ChoosePenCommand { get; private set; }
        public RelayCommand ChooseEraserCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand OpenCommand { get; private set; }
        public RelayCommand BroadcastChangedCommand { get; private set; }
        public RelayCommand ReceiveChangedCommand { get; private set; }

        private IOpenSaveDialogService _dialogService;
        private INetworkBroadcaster _broadcastService;
        private ConcurrentBag<Stroke> _receivedStrokes = new ConcurrentBag<Stroke>();
        private CancellationTokenSource _receiveTokenSource;

        public PaintViewModel(INetworkBroadcaster broadcastService, IOpenSaveDialogService dialogService)
        {
            _broadcastService = broadcastService;
            _dialogService = dialogService;

            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;

            ClearCommand = new RelayCommand(async (param) => await ClearCommandHandler(param), param => Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand(param => IsEraser = false, param => IsEraser);
            ChooseEraserCommand = new RelayCommand(param => IsEraser = true, param => !IsEraser);
            SaveCommand = new RelayCommand(OnSaveExecuted);
            OpenCommand = new RelayCommand(OnOpenExecuted);
            BroadcastChangedCommand = new RelayCommand(param => OnBroadcastChanged(param));
            ReceiveChangedCommand = new RelayCommand(async (param) =>
            {
                try
                {
                    await OnReceiveChanged(param);
                }
                catch (OperationCanceledException)
                { }
            });
            PropertyChanged += PropertyChangedHandler;
        }

        private void PropertyChangedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (IsBroadcast && e.PropertyName == nameof(Background))
            {
                var info = new DrawingInfo(ARGBColor.FromColor(Background), SerializableStroke.Default, IsEraser);
                var serializer = new BinaryFormatter();
                var bytes = serializer.OneLineSerialize(info);
                _broadcastService.SendAsync(bytes).SafeFireAndForget();
            }
        }

        private void OnBroadcastChanged(object param)
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

        private async ValueTask OnReceiveChanged(object param)
        {
            if (IsReceive)
            {
                _receiveTokenSource = new CancellationTokenSource();
                await Receive(_receiveTokenSource.Token);
            }
            else
            {
                _receiveTokenSource.Cancel();
            }
        }

        private async ValueTask ClearCommandHandler(object obj)
        {
            Strokes.Clear();
            _receivedStrokes.Clear();

            if (IsBroadcast)
            {
                var info = new DrawingInfo(ARGBColor.Default, SerializableStroke.Default, IsEraser, true);
                var serializer = new BinaryFormatter();
                var bytes = serializer.OneLineSerialize(info);
                await _broadcastService.SendAsync(bytes);
            }
        }

        private void OnSaveExecuted(object param)
        {
            var savePath = _dialogService.SaveFileDialog();
            //TODO: Save all drawing data into object, serialize it and save to file
        }

        private void OnOpenExecuted(object param)
        {
            var openPath = _dialogService.OpenFileDialog();
            //TODO: Add suggestion to save current work in case board not empty???
            //TODO: Read file, deserialize to object and apply to current border
        }

        private Task Receive(CancellationToken token)
        {
            return Task.Run(async () =>
            {
                await _broadcastService.ClearBufferAsync();
                while (true)
                {
                    var data = await _broadcastService.ReceiveAsync().WithCancellation(token);

                    if (data == null || data.Length == 0)
                    {
                        continue;
                    }

                    var binarySerializer = new BinaryFormatter();
                    var info = binarySerializer.OneLineDeserialize<DrawingInfo>(data);

                    if (info.ClearBoard)
                    {
                        Application.Current.Dispatcher.Invoke(() => Strokes.Clear());
                        _receivedStrokes.Clear();
                        continue;
                    }

                    if (info.Background != ARGBColor.FromColor(Background) && info.Stroke == SerializableStroke.Default)
                    {
                        Background = info.Background.AsColor();
                    }

                    if (!info.Stroke.Equals(SerializableStroke.Default))
                    {
                        var stroke = info.Stroke.ToStroke();

                        if (info.IsEraser)
                        {
                            stroke.DrawingAttributes.Color = Background;
                        }

                        _receivedStrokes.Add(stroke);
                        Application.Current.Dispatcher.Invoke(() => Strokes.Add(stroke));
                    }
                }
            }, token);
        }

        private async void OnStrokesCollectionChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (e.Added.Count > 0)
            {
                if (IsBroadcast)
                {
                    foreach (var stroke in e.Added)
                    {
                        if (!_receivedStrokes.Contains(stroke))
                        {
                            var serializableStroke = SerializableStroke.FromStroke(stroke);
                            var info = new DrawingInfo(Background, serializableStroke, IsEraser);
                            var serializer = new BinaryFormatter();
                            var bytes = serializer.OneLineSerialize(info);
                            await _broadcastService.SendAsync(bytes);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            _broadcastService.Dispose();
        }
    }
}
