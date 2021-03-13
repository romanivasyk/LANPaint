using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Ink;

namespace LANPaint.Model
{
    [Serializable]
    public readonly struct SerializableStroke : IEquatable<SerializableStroke>
    {
        [NonSerialized]
        public static readonly SerializableStroke Default = new SerializableStroke(new StrokeAttributes());

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
}