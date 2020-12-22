using System;
using System.Windows.Media;
using System.Windows.Ink;
using System.Collections.Generic;
using System.Windows;

namespace LANPaint_vNext.Model
{
    [Serializable]
    public readonly struct DrawingInfo
    {
        public ARGBColor Background { get; }
        public bool IsEraser { get; }
        public bool ClearBoard { get; }
        public SerializableStroke Stroke { get; }

        public DrawingInfo(Color background, SerializableStroke stroke,
                           bool isEraser = false, bool clearBoard = false)
        {
            Background = new ARGBColor(background.A, background.R, background.G, background.B);
            Stroke = stroke;
            IsEraser = isEraser;
            ClearBoard = clearBoard;
        }

        public override string ToString()
        {
            return $"Background:{Background}, IsEraser:{IsEraser}, ClearBoard:{ClearBoard}, Stroke:{Stroke}";
        }

        [Serializable]
        public readonly struct ARGBColor
        {
            public byte A { get; }
            public byte R { get; }
            public byte G { get; }
            public byte B { get; }

            public ARGBColor(byte a, byte r, byte g, byte b)
            {
                A = a;
                R = r;
                G = g;
                B = b;
            }

            public readonly Color AsColor()
            {
                return Color.FromArgb(A, R, G, B);
            }
        }

        public class SerializableStroke
        {
            public IEnumerable<Point> Points { get; }
            public StrokeAttributes Attributes { get; }
        }

        public readonly struct StrokeAttributes
        {
            public ARGBColor Color { get; }
            public double Width { get; }
            public double Height { get; }
            public bool IgnorePressure { get; }
            public bool IsHighlighter { get; }
            public StylusTip StylusTip { get; }
        }
    }
}
