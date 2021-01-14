using LANPaint_vNext.Extensions;
using LANPaint_vNext.Model;
using LANPaint_vNext.Services;
using LANPaint_vNext.Services.UDP;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;

namespace LANPaint_vNext.ViewModels
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

        private IDialogWindowService _dialogService;
        private BroadcastChainer _broadcastService = new BroadcastChainer();
        private ConcurrentBag<Stroke> _receivedStrokes = new ConcurrentBag<Stroke>();
        private CancellationTokenSource _receiveTokenSource = new CancellationTokenSource();

        public PaintViewModel(IDialogWindowService dialogService)
        {
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
                {
                    _receiveTokenSource = new CancellationTokenSource();
                }
            });
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
            if (IsBroadcast)
            {
                var info = new DrawingInfo(ARGBColor.Default, new SerializableStroke(), IsEraser, true);
                var serializer = new BinarySerializerService();
                var bytes = serializer.Serialize(info);
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
            void ClearBuffer(CancellationToken ct)
            {
                Task.Run(() =>
                {
                    while (true)
                    {
                        _broadcastService.ReceiveAsync().WithCancellation(ct).GetAwaiter().GetResult();
                    }
                }, ct);
            }

            return Task.Run(() =>
            {
                //This trash clears Socket Buffer
                var tokenSource = new CancellationTokenSource();
                ClearBuffer(tokenSource.Token);
                Task.Delay(500).Wait();
                tokenSource.Cancel();

                while (true)
                {
                    var data = _broadcastService.ReceiveAsync().WithCancellation(token).GetAwaiter().GetResult();

                    if (data == null)
                    {
                        continue;
                    }

                    var binarySerializer = new BinarySerializerService();
                    var info = binarySerializer.Deserialize<DrawingInfo>(data);

                    if (info.ClearBoard)
                    {
                        Application.Current.Dispatcher.Invoke(() => Strokes.Clear());
                        continue;
                    }

                    var receivedStroke = info.Stroke;

                    var stroke = new Stroke(new System.Windows.Input.StylusPointCollection(info.Stroke.Points),
                        new DrawingAttributes
                        {
                            Color = receivedStroke.Attributes.Color.AsColor(),
                            Height = receivedStroke.Attributes.Height,
                            Width = receivedStroke.Attributes.Width,
                            IgnorePressure = receivedStroke.Attributes.IgnorePressure,
                            IsHighlighter = receivedStroke.Attributes.IsHighlighter,
                            StylusTip = receivedStroke.Attributes.StylusTip
                        });

                    if (info.IsEraser)
                    {
                        stroke.DrawingAttributes.Color = Background;
                    }

                    _receivedStrokes.Add(stroke);
                    Application.Current.Dispatcher.Invoke(() => Strokes.Add(stroke));
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
                            var attr = new StrokeAttributes
                            {
                                Color = ARGBColor.FromColor(stroke.DrawingAttributes.Color),
                                Height = stroke.DrawingAttributes.Height,
                                Width = stroke.DrawingAttributes.Width,
                                StylusTip = stroke.DrawingAttributes.StylusTip
                            };
                            var points = new List<Point>();
                            foreach (var point in stroke.StylusPoints)
                            {
                                points.Add(point.ToPoint());
                            }

                            var serializableStroke = new SerializableStroke(attr, points);
                            var info = new DrawingInfo(Background, serializableStroke, IsEraser);
                            var serializer = new BinarySerializerService();
                            var bytes = serializer.Serialize(info);
                            await _broadcastService.SendAsync(bytes);
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            _broadcastService?.Dispose();
        }
    }
}
