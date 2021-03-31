using System;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace LANPaint.Model
{
    [Serializable]
    public readonly struct SerializableStroke : IEquatable<SerializableStroke>
    {
        public StrokeAttributes Attributes { get; }
        public ReadOnlyCollection<Point> Points { get; }

        public SerializableStroke(StrokeAttributes attributes, Point[] points)
        {
            if (points is null)
                throw new ArgumentNullException(nameof(points), "SerializableStroke cannot contain null points collection");
            if (points.Length < 1)
                throw new ArgumentException("SerializableStroke cannot contain empty points collection", nameof(points));
            Attributes = attributes;
            Points = new ReadOnlyCollection<Point>(points);
        }

        public static SerializableStroke FromStroke(Stroke stroke)
        {
            var attr = new StrokeAttributes
            {
                Color = ARGBColor.FromColor(stroke.DrawingAttributes.Color),
                Height = stroke.DrawingAttributes.Height,
                Width = stroke.DrawingAttributes.Width,
                IgnorePressure = stroke.DrawingAttributes.IgnorePressure,
                IsHighlighter = stroke.DrawingAttributes.IsHighlighter,
                StylusTip = stroke.DrawingAttributes.StylusTip
            };

            var points = stroke.StylusPoints.Select(point => point.ToPoint()).ToArray();
            return new SerializableStroke(attr, points);
        }

        public Stroke ToStroke() => new(new System.Windows.Input.StylusPointCollection(Points),
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

        public bool Equals([DisallowNull] SerializableStroke other)
        {
            return Attributes.Equals(other.Attributes) && Points.SequenceEqual(other.Points);
        }

        public override bool Equals([AllowNull] object obj) => obj is SerializableStroke stroke && Equals(stroke);

        public override int GetHashCode()
        {
            var pointsHash = Points.Aggregate<Point, int>(int.MaxValue,
                (accumulator, point) => accumulator ^= point.X.GetHashCode() ^ point.Y.GetHashCode());
            return Attributes.GetHashCode() ^ pointsHash;
        }
    }
}