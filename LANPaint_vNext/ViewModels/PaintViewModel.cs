using LANPaint_vNext.Model;
using LANPaint_vNext.Services;
using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Threading;
using System.Linq;

namespace LANPaint_vNext.ViewModels
{
    public class PaintViewModel : BindableBase, IDisposable
    {
        private bool _isEraser;
        private StrokeCollection _strokes;
        private Color _backgroundColor;

        public bool IsEraser
        {
            get { return _isEraser; }
            set
            {
                SetProperty(ref _isEraser, value);
            }
        }
        public StrokeCollection Strokes
        {
            get => _strokes;
            set
            {
                SetProperty(ref _strokes, value);
            }
        }
        public Color Background
        {
            get => _backgroundColor;
            set
            {
                SetProperty(ref _backgroundColor, value);
            }
        }
        public bool BroadcastEnabled { get; set; }

        private CancellationTokenSource _receiveCancellationSource;

        private bool _receiveEnabled;
        public bool ReceiveEnabled
        {
            get => _receiveEnabled;
            set
            {
                if (_receiveEnabled == value)
                {
                    return;
                }

                if (value)
                {
                    _receiveCancellationSource = new CancellationTokenSource();
                    Receive(_receiveCancellationSource.Token, Dispatcher.CurrentDispatcher);
                    _receiveEnabled = value;
                    //TODO: Setup CancellationToken and start a Task to receive packets. Probably we should synchronize Strokes collection
                    //before adding received stroke
                }
                else
                {
                    //TODO: Use CancellationToken to cancel the receiving task(is it possible in case the thread is blocked by synchrornys Receive or await???)
                    _receiveCancellationSource?.Cancel();
                    _receiveEnabled = value;
                }
            }
        }

        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand ChoosePenCommand { get; private set; }
        public RelayCommand ChooseEraserCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand OpenCommand { get; private set; }

        private IDialogWindowService _dialogService;
        private UDPBroadcastService _broadcastService;
        private ConcurrentBag<Stroke> _receivedStrokes = new ConcurrentBag<Stroke>();

        public PaintViewModel(IDialogWindowService dialogService)
        {
            _dialogService = dialogService;
            _broadcastService = new UDPBroadcastService();

            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;
            ClearCommand = new RelayCommand(async (param) => await ClearCommandHandler(param), param => Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand(param => IsEraser = false, param => IsEraser);
            ChooseEraserCommand = new RelayCommand(param => IsEraser = true, param => !IsEraser);
            SaveCommand = new RelayCommand(OnSaveExecuted);
            OpenCommand = new RelayCommand(OnOpenExecuted);
        }

        private async ValueTask ClearCommandHandler(object obj)
        {
            Strokes.Clear();
            if (BroadcastEnabled)
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

        private Task Receive(CancellationToken token, Dispatcher dispatcher)
        {
            return Task.Run(() =>
            {
                while (true)
                {
                    var data = _broadcastService.ReceiveAsync().WithCancellation(token).GetAwaiter().GetResult();

                    if (_receiveEnabled)
                    {
                        if(data == null)
                        {
                            continue;
                        }

                        var binarySerializator = new BinarySerializerService();
                        var info = binarySerializator.Deserialize<DrawingInfo>(data);
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

                        _receivedStrokes.Add(stroke);
                        dispatcher.Invoke(() => Strokes.Add(stroke));
                    }
                    else
                    {
                        throw new TaskCanceledException();
                    }
                }
            }, token);
        }

        private async void OnStrokesCollectionChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (e.Added.Count > 0)
            {
                Debug.WriteLine($"Strokes added: {e.Added.Count}");
                if (BroadcastEnabled)
                {
                    Debug.WriteLine($"Sending stroke...");
                    foreach (var stroke in e.Added)
                    {
                        if(!_receivedStrokes.Contains(stroke))
                        {
                            var attr = new StrokeAttributes
                            {
                                Color = ARGBColor.FromColor(stroke.DrawingAttributes.Color),
                                Height = stroke.DrawingAttributes.Height,
                                Width = stroke.DrawingAttributes.Width
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
            if (e.Removed.Count > 0)
            {
                Debug.WriteLine($"Strokes removed: {e.Removed.Count}");
            }
        }

        public void Dispose()
        {
            _broadcastService?.Dispose();
            _receiveCancellationSource?.Dispose();
        }
    }
}
