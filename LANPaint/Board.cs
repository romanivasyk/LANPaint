using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace LANPaint
{
    public class Board : InkCanvas
    {
        public static readonly DependencyProperty MousePositionProperty = DependencyProperty.Register(
            nameof(MousePosition), typeof(Point), typeof(Board));

        public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
            nameof(StrokeThickness), typeof(double), typeof(Board),
            new FrameworkPropertyMetadata(default(double), FrameworkPropertyMetadataOptions.AffectsRender, OnThicknessChanged));

        public static readonly DependencyProperty StrokeColorProperty = DependencyProperty.Register(
            nameof(StrokeColor), typeof(Color), typeof(Board),
            new FrameworkPropertyMetadata(default(Color), FrameworkPropertyMetadataOptions.AffectsRender, OnColorChanged));

        public static readonly DependencyProperty IsEraserProperty = DependencyProperty.Register(
            nameof(IsEraser), typeof(bool), typeof(Board),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender, OnIsEraserChanged));

        public static readonly DependencyProperty EraserCursorProperty = DependencyProperty.Register(
            nameof(EraserCursor), typeof(Cursor), typeof(Board),
            new PropertyMetadata(GetDefaultEraserCursor()));

        public Point MousePosition
        {
            get => (Point)GetValue(MousePositionProperty);
            private set => SetValue(MousePositionProperty, value);
        }

        public double StrokeThickness
        {
            get => DefaultDrawingAttributes.Width;
            set => SetValue(StrokeThicknessProperty, value);
        }

        public Color StrokeColor
        {
            get => DefaultDrawingAttributes.Color;
            set => SetValue(StrokeColorProperty, value);
        }

        public bool IsEraser
        {
            get => (bool)GetValue(IsEraserProperty);
            set => SetValue(IsEraserProperty, value);
        }

        public Cursor EraserCursor
        {
            get => (Cursor)GetValue(EraserCursorProperty);
            set => SetValue(EraserCursorProperty, value);
        }

        public new DrawingAttributes DefaultDrawingAttributes
        {
            get => base.DefaultDrawingAttributes;
            private set => base.DefaultDrawingAttributes = value;
        }

        public new bool UseCustomCursor 
        {
            get => base.UseCustomCursor;
            private set => base.UseCustomCursor = value; 
        }

        //Field used to store StrokeColor in case erasing mode enabled
        //and restore it after erasing mode disabled
        private Color _cachedStrokeColor;

        //Store strokes used as eraser to change their color in case
        //background color will be changed
        private List<Stroke> _eraserStrokes;

        static Board()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Board), new FrameworkPropertyMetadata(typeof(Board)));
        }

        public Board()
        {
            _cachedStrokeColor = DefaultDrawingAttributes.Color;
            _eraserStrokes = new List<Stroke>();
            UseCustomCursor = true;
            Cursor = Cursors.Pen;
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            StrokeThickness = 2;
            StrokeColor = Color.FromRgb(0, 0, 0);
            DefaultDrawingAttributes.IgnorePressure = true;

            var overridedMetadata = new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnBackgroundChanged);
            BackgroundProperty.OverrideMetadata(typeof(Board), overridedMetadata);
        }

        protected override void OnStrokesReplaced(InkCanvasStrokesReplacedEventArgs e)
        {
            base.OnStrokesReplaced(e);
            if (e.PreviousStrokes != null)
            {
                e.PreviousStrokes.StrokesChanged -= OnStrokesChanged;
            }
            e.NewStrokes.StrokesChanged += OnStrokesChanged;
        }

        private void OnStrokesChanged(object sender, StrokeCollectionChangedEventArgs e)
        {
            if (e.Added.Count > 0)
            {
                if (IsEraser)
                {
                    _eraserStrokes.AddRange(e.Added);
                }
            }
            if (e.Removed.Count > 0)
            {
                foreach (var item in e.Removed)
                {
                    _eraserStrokes.Remove(item);
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            MousePosition = e.GetPosition(this);
        }

        private void OnBackgroundChanged(DependencyObject boardControl, DependencyPropertyChangedEventArgs e)
        {
            var control = (Board)boardControl;
            var newBackgroundColor = ((SolidColorBrush)e.NewValue).Color;

            if (IsEraser)
            {
                control.DefaultDrawingAttributes.Color = newBackgroundColor;
            }
            _eraserStrokes.ForEach(stroke => stroke.DrawingAttributes.Color = newBackgroundColor);
        }

        private static void OnThicknessChanged(DependencyObject boardControl, DependencyPropertyChangedEventArgs e)
        {
            var control = (Board)boardControl;
            control.DefaultDrawingAttributes.Width = (double)e.NewValue;
            control.DefaultDrawingAttributes.Height = (double)e.NewValue;
        }

        private static void OnColorChanged(DependencyObject boardControl, DependencyPropertyChangedEventArgs e)
        {
            var control = (Board)boardControl;
            control.DefaultDrawingAttributes.Color = (Color)e.NewValue;
        }

        private static void OnIsEraserChanged(DependencyObject boardControl, DependencyPropertyChangedEventArgs e)
        {
            var control = (Board)boardControl;
            if ((bool)e.NewValue)
            {
                control._cachedStrokeColor = control.DefaultDrawingAttributes.Color;
                control.DefaultDrawingAttributes.Color = ((SolidColorBrush)control.Background).Color;
                control.Cursor = control.EraserCursor;
            }
            else
            {
                control.DefaultDrawingAttributes.Color = control._cachedStrokeColor;
                control.Cursor = Cursors.Pen;
            }
        }

        private static object GetDefaultEraserCursor()
        {
            try
            {
                var eraserCursorResourceStream = Application.GetResourceStream(new Uri("Resources/eraser.cur", UriKind.Relative));
                return eraserCursorResourceStream != null ? new Cursor(eraserCursorResourceStream.Stream) : Cursors.Arrow;
            }
            catch
            {
                return Cursors.Arrow;
            }
        }
    }
}
