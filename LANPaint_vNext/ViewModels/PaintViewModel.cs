using System.Diagnostics;
using System.Windows.Ink;
using System.Windows.Media;

namespace LANPaint_vNext.ViewModels
{
    public class PaintViewModel : BindableBase
    {
        private Color _color;
        public Color Color
        {
            get => _color;
            set
            {
                _color = value;
                NotifyPropertyChanged();
            }
        }

        private bool _isEraser;
        public bool IsEraser
        {
            get { return _isEraser; }
            set
            {
                _isEraser = value;
                NotifyPropertyChanged();
            }
        }

        private StrokeCollection _strokes;
        public StrokeCollection Strokes
        {
            get => _strokes;
            set
            {
                _strokes = value;
                NotifyPropertyChanged();
            }
        }

        public RelayCommand ClearCommand { get; private set; }
        public RelayCommand ChoosePenCommand { get; private set; }
        public RelayCommand ChooseEraserCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand OpenCommand { get; private set; }

        public PaintViewModel()
        {
            Color = Color.FromRgb(255, 10, 10);
            Strokes = new StrokeCollection();
            Strokes.StrokesChanged += OnStrokesCollectionChanged;
            ClearCommand = new RelayCommand(param => Strokes.Clear(), param => Strokes.Count > 0);
            ChoosePenCommand = new RelayCommand(param => IsEraser = false, param => IsEraser);
            ChooseEraserCommand = new RelayCommand(param => IsEraser = true, param => !IsEraser);
            SaveCommand = new RelayCommand(OnSaveExecuted);
            OpenCommand = new RelayCommand(OnOpenExecuted);
        }

        private void OnSaveExecuted(object param)
        {
            Debug.WriteLine("State saved");
        }

        private void OnOpenExecuted(object param)
        {
            Debug.WriteLine("State opened");
        }

        private void OnStrokesCollectionChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            Debug.WriteLine($"Strokes added: {e.Added.Count}");
            Debug.WriteLine($"Strokes removed: {e.Removed.Count}");
        }
    }
}
