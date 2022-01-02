using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Media;

namespace LANPaint.Model;

[Serializable]
public readonly struct ARGBColor : IEquatable<ARGBColor>
{
    [NonSerialized]
    public static readonly ARGBColor Default = new(255, 0, 0, 0);

    public byte A { get; init; }
    public byte R { get; init; }
    public byte G { get; init; }
    public byte B { get; init; }

    public ARGBColor(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public Color AsColor() => Color.FromArgb(A, R, G, B);

    public static ARGBColor FromColor(Color color) => new(color.A, color.R, color.G, color.B);

    public bool Equals([AllowNull] ARGBColor other) => A == other.A && R == other.R && G == other.G && B == other.B;

    public override bool Equals(object obj) => obj is ARGBColor color && Equals(color);

    public override int GetHashCode() => A.GetHashCode() ^ R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();

    public static bool operator ==(ARGBColor color, ARGBColor other) => color.Equals(other);

    public static bool operator !=(ARGBColor color, ARGBColor other) => !color.Equals(other);

    public override string ToString() => $"A:{A}, R:{R}, G:{G}, B:{B}";
}