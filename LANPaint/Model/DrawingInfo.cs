using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Media;

namespace LANPaint.Model
{
    [Serializable]
    public readonly struct ARGBColor : IEquatable<ARGBColor>
    {
        [NonSerialized]
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

        public readonly Color AsColor() => Color.FromArgb(A, R, G, B);

        public static ARGBColor FromColor(Color color) => new ARGBColor(color.A, color.R, color.G, color.B);

        public bool Equals([AllowNull] ARGBColor other) => A == other.A && R == other.R && G == other.G && B == other.B;

        public override bool Equals(object obj) => obj is ARGBColor color && Equals(color);

        public override int GetHashCode() => A.GetHashCode() ^ R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();

        public static bool operator ==(ARGBColor color, ARGBColor other) => color.Equals(other);

        public static bool operator !=(ARGBColor color, ARGBColor other) => !color.Equals(other);

        public override string ToString() => $"A:{A}, R:{R}, G:{G}, B:{B}";
    }

    [Serializable]
    public readonly struct SerializableStroke : IEquatable<SerializableStroke>
    {
        [NonSerialized]
        public static readonly SerializableStroke Default = new SerializableStroke(new StrokeAttributes(), null);

        public IEnumerable<Point> Points { get; }
        public StrokeAttributes Attributes { get; }

        public SerializableStroke(StrokeAttributes attributes, IEnumerable<Point> points = null)
        {
            Attributes = attributes;
            Points = points;
        }

        public static SerializableStroke FromStroke(Stroke stroke)
        {
            var attr = new StrokeAttributes
            {
                Color = ARGBColor.FromColor(stroke.DrawingAttributes.Color),
                Height = stroke.DrawingAttributes.Height,
                Width = stroke.DrawingAttributes.Width,
                StylusTip = stroke.DrawingAttributes.StylusTip
            };

            var points = stroke.StylusPoints.Select(point => point.ToPoint()).ToList();
            return new SerializableStroke(attr, points);
        }

        public readonly Stroke ToStroke() => new Stroke(new System.Windows.Input.StylusPointCollection(Points),
                            new DrawingAttributes
                            {
                                Color = Attributes.Color.AsColor(),
                                Height = Attributes.Height,
                                Width = Attributes.Width,
                                IgnorePressure = Attributes.IgnorePressure,
                                IsHighlighter = Attributes.IsHighlighter,
                                StylusTip = Attributes.StylusTip
                            });

        public static bool operator ==(SerializableStroke stroke, SerializableStroke other) => stroke.Equals(other);

        public static bool operator !=(SerializableStroke stroke, SerializableStroke other) => !stroke.Equals(other);

        public bool Equals([AllowNull] SerializableStroke other)
        {
            if (Points != null && other.Points != null)
            {
                return Attributes.Equals(other.Attributes) && Points.SequenceEqual(other.Points);
            }
            if (Points == null && other.Points == null)
            {
                return Attributes.Equals(other.Attributes);
            }
            return false;
        }

        public override bool Equals(object obj) => obj is SerializableStroke stroke && Equals(stroke);

        public override int GetHashCode()
        {
            var pointsHash = Points.Aggregate<Point, int>(default, (current, point) => current + (point.GetHashCode() ^ current));
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
