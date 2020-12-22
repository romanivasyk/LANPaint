using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;
using System.Linq;

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
                           bool isEraser = false, bool clearBoard = false) : this(stroke, isEraser, clearBoard)
        {
            Background = ARGBColor.FromColor(background);
        }

        public DrawingInfo(ARGBColor background, SerializableStroke stroke,
                   bool isEraser = false, bool clearBoard = false) : this(stroke, isEraser, clearBoard)
        {
            Background = background;
        }

        private DrawingInfo(SerializableStroke stroke, bool isEraser = false, bool clearBoard = false)
        {
            Background = ARGBColor.Default;
            Stroke = stroke;
            IsEraser = isEraser;
            ClearBoard = clearBoard;
        }

        public override string ToString()
        {
            return $"Background:{Background}, IsEraser:{IsEraser}, ClearBoard:{ClearBoard}, Stroke:{Stroke}";
        }
    }

    [Serializable]
    public readonly struct ARGBColor
    {
        public static readonly ARGBColor Default = new ARGBColor(255, 0, 0, 0);
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

        public static ARGBColor FromColor(Color color)
        {
            return new ARGBColor(color.A, color.R, color.G, color.B);
        }

        public override string ToString()
        {
            return $"A:{A}, R:{R}, G:{G}, B:{B}";
        }
    }

    [Serializable]
    public readonly struct SerializableStroke
    {
        public IEnumerable<Point> Points { get; }
        public StrokeAttributes Attributes { get; }

        public SerializableStroke(StrokeAttributes attributes, IEnumerable<Point> points = null)
        {
            Attributes = attributes;
            Points = points;
        }

        public override bool Equals(object obj)
        {
            return obj is SerializableStroke stroke ? Attributes.Equals(stroke.Attributes) && Points.SequenceEqual(stroke.Points) : false;
        }

        public override int GetHashCode()
        {
            int pointsHash = default;
            foreach (var point in Points)
            {
                pointsHash += point.GetHashCode() ^ pointsHash;
            }

            return Attributes.GetHashCode() ^ pointsHash;
        }
    }

    [Serializable]
    public struct StrokeAttributes
    {
        public ARGBColor Color { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IgnorePressure { get; set; }
        public bool IsHighlighter { get; set; }
        public StylusTip StylusTip { get; set; }
    }
}
