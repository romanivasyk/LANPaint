using System;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using LANPaint.Model;
using Xunit;

namespace LANPaint.UnitTests.Model;

public class SerializableStrokesTests
{
    private readonly StrokeAttributes _attributes;
    private readonly Point[] _points;

    private readonly Stroke _nonSerializableStroke;

    public SerializableStrokesTests()
    {
        _attributes = new StrokeAttributes
        {
            Color = ARGBColor.Default,
            Height = 2,
            Width = 2,
            IgnorePressure = true,
            IsHighlighter = false,
            StylusTip = StylusTip.Ellipse
        };
        _points = Enumerable.Range(10, 100).Select(i => new Point(i, i)).ToArray();
        _nonSerializableStroke = new Stroke(new StylusPointCollection(_points),
            new DrawingAttributes
            {
                Color = _attributes.Color.AsColor(),
                Height = _attributes.Height,
                Width = _attributes.Width,
                IgnorePressure = _attributes.IgnorePressure,
                IsHighlighter = _attributes.IsHighlighter,
                StylusTip = _attributes.StylusTip
            });
    }

    [Fact]
    public void Ctor_ValidData()
    {
        var stroke = new SerializableStroke(_attributes, _points);

        Assert.Equal(stroke.Attributes, _attributes);
        Assert.True(stroke.Points.SequenceEqual(_points));
    }

    [Fact]
    public void Ctor_NullPointCollection()
    {
        Assert.Throws<ArgumentNullException>(() => new SerializableStroke(_attributes, null));
    }

    [Fact]
    public void Ctor_EmptyPointCollection()
    {
        Assert.Throws<ArgumentException>(() => new SerializableStroke(_attributes, Array.Empty<Point>()));
    }

    [Fact]
    public void OverridenEqualsForEqualStrokes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var strokeInstanceResult = stroke.Equals((object) anotherStroke);
        var anotherStrokeInstanceResult = anotherStroke.Equals((object) stroke);

        Assert.True(strokeInstanceResult);
        Assert.True(anotherStrokeInstanceResult);
    }

    [Fact]
    public void OverridenEqualsForNonEqualStrokes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        anotherPoints[0] = new Point(anotherPoints[0].X + 1, anotherPoints[0].Y - 1);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var strokeInstanceResult = stroke.Equals((object) anotherStroke);
        var anotherStrokeInstanceResult = anotherStroke.Equals((object) stroke);

        Assert.False(strokeInstanceResult);
        Assert.False(anotherStrokeInstanceResult);
    }

    [Fact]
    public void OverridenEqualsPassNull()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var result = stroke.Equals(null);
        Assert.False(result);
    }

    [Fact]
    public void EqualsForEqualStrokes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var strokeInstanceResult = stroke.Equals(anotherStroke);
        var anotherStrokeInstanceResult = anotherStroke.Equals(stroke);

        Assert.True(strokeInstanceResult);
        Assert.True(anotherStrokeInstanceResult);
    }

    [Fact]
    public void EqualsForNonEqualStrokesByPoints()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        anotherPoints[0] = new Point(anotherPoints[0].X + 1, anotherPoints[0].Y - 1);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var strokeInstanceResult = stroke.Equals(anotherStroke);
        var anotherStrokeInstanceResult = anotherStroke.Equals(stroke);

        Assert.False(strokeInstanceResult);
        Assert.False(anotherStrokeInstanceResult);
    }

    [Fact]
    public void EqualsForNonEqualStrokesByAttributes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        var anotherAttributes = new StrokeAttributes
        {
            Color = ARGBColor.Default,
            Height = stroke.Attributes.Height + 2,
            Width = 2,
            IgnorePressure = true,
            IsHighlighter = !stroke.Attributes.IsHighlighter,
            StylusTip = StylusTip.Ellipse
        };
        var anotherStroke = new SerializableStroke(anotherAttributes, anotherPoints);

        var strokeInstanceResult = stroke.Equals(anotherStroke);
        var anotherStrokeInstanceResult = anotherStroke.Equals(stroke);

        Assert.False(strokeInstanceResult);
        Assert.False(anotherStrokeInstanceResult);
    }

    [Fact]
    public void EqualsPassNull()
    {
        IEquatable<SerializableStroke> stroke = new SerializableStroke(_attributes, _points);
        SerializableStroke? anotherStroke = null;
        var result = stroke.Equals(anotherStroke);
        Assert.False(result);
    }

    [Fact]
    public void EqualityOperatorsForEqualsStrokes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var equalityResult = stroke == anotherStroke;
        var nonEqualityResult = stroke != anotherStroke;

        Assert.True(equalityResult);
        Assert.False(nonEqualityResult);
    }

    [Fact]
    public void EqualityOperatorForNonEqualsStrokesByPoints()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        anotherPoints[0] = new Point(anotherPoints[0].X + 1, anotherPoints[0].Y - 1);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var equalityResult = stroke == anotherStroke;
        var nonEqualityResult = stroke != anotherStroke;

        Assert.False(equalityResult);
        Assert.True(nonEqualityResult);
    }

    [Fact]
    public void EqualityOperatorForNonEqualsStrokesByAttributes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        var anotherAttributes = new StrokeAttributes
        {
            Color = ARGBColor.Default,
            Height = stroke.Attributes.Height + 2,
            Width = 2,
            IgnorePressure = true,
            IsHighlighter = !stroke.Attributes.IsHighlighter,
            StylusTip = StylusTip.Ellipse
        };
        var anotherStroke = new SerializableStroke(anotherAttributes, anotherPoints);

        var equalityResult = stroke == anotherStroke;
        var nonEqualityResult = stroke != anotherStroke;

        Assert.False(equalityResult);
        Assert.True(nonEqualityResult);
    }

    [Fact]
    public void GetHashCodeForEqualStrokes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var strokeHash = stroke.GetHashCode();
        var anotherStrokeHash = anotherStroke.GetHashCode();

        Assert.Equal(strokeHash, anotherStrokeHash);
    }

    [Fact]
    public void GetHashCodeForNonEqualStrokesByPoints()
    {
        var stroke = new SerializableStroke(_attributes, _points);

        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        anotherPoints[0] = new Point(anotherPoints[0].X + 1, anotherPoints[0].Y - 1);
        var anotherStroke = new SerializableStroke(_attributes, anotherPoints);

        var strokeHash = stroke.GetHashCode();
        var anotherStrokeHash = anotherStroke.GetHashCode();

        Assert.NotEqual(strokeHash, anotherStrokeHash);
    }

    [Fact]
    public void GetHashCodeForNonEqualStrokesByAttributes()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var anotherPoints = new Point[_points.Length];
        _points.CopyTo(anotherPoints, 0);
        var anotherAttributes = new StrokeAttributes
        {
            Color = ARGBColor.Default,
            Height = stroke.Attributes.Height + 2,
            Width = 2,
            IgnorePressure = true,
            IsHighlighter = !stroke.Attributes.IsHighlighter,
            StylusTip = StylusTip.Ellipse
        };
        var anotherStroke = new SerializableStroke(anotherAttributes, anotherPoints);

        var strokeHash = stroke.GetHashCode();
        var anotherStrokeHash = anotherStroke.GetHashCode();

        Assert.NotEqual(strokeHash, anotherStrokeHash);
    }

    [Fact]
    public void ToStroke()
    {
        var stroke = new SerializableStroke(_attributes, _points);
        var asNonSerializable = stroke.ToStroke();

        Assert.True(stroke.Points.SequenceEqual(asNonSerializable.StylusPoints.Select(sPoint => sPoint.ToPoint())));
        Assert.Equal(stroke.Attributes.Color.AsColor(), asNonSerializable.DrawingAttributes.Color);
        Assert.Equal(stroke.Attributes.Height, asNonSerializable.DrawingAttributes.Height);
        Assert.Equal(stroke.Attributes.Width, asNonSerializable.DrawingAttributes.Width);
        Assert.Equal(stroke.Attributes.IgnorePressure, asNonSerializable.DrawingAttributes.IgnorePressure);
        Assert.Equal(stroke.Attributes.IsHighlighter, asNonSerializable.DrawingAttributes.IsHighlighter);
        Assert.Equal(stroke.Attributes.StylusTip, asNonSerializable.DrawingAttributes.StylusTip);
    }

    [Fact]
    public void FromStroke()
    {
        var stroke = SerializableStroke.FromStroke(_nonSerializableStroke);

        Assert.True(stroke.Points.SequenceEqual(_nonSerializableStroke.StylusPoints.Select(sPoint => sPoint.ToPoint())));
        Assert.Equal(stroke.Attributes.Color.AsColor(), _nonSerializableStroke.DrawingAttributes.Color);
        Assert.Equal(stroke.Attributes.Height, _nonSerializableStroke.DrawingAttributes.Height);
        Assert.Equal(stroke.Attributes.Width, _nonSerializableStroke.DrawingAttributes.Width);
        Assert.Equal(stroke.Attributes.IgnorePressure, _nonSerializableStroke.DrawingAttributes.IgnorePressure);
        Assert.Equal(stroke.Attributes.IsHighlighter, _nonSerializableStroke.DrawingAttributes.IsHighlighter);
        Assert.Equal(stroke.Attributes.StylusTip, _nonSerializableStroke.DrawingAttributes.StylusTip);
    }

    [Fact]
    public void FromStrokePassNull()
    {
        Assert.Throws<ArgumentNullException>(() => SerializableStroke.FromStroke(null));
    }
}