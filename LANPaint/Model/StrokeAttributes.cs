using System;
using System.Windows.Ink;

namespace LANPaint.Model
{
    [Serializable]
    public struct StrokeAttributes
    {
        public ARGBColor Color { get; init; }
        public double Width { get; init; }
        public double Height { get; init; }
        public bool IgnorePressure { get; init; }
        public bool IsHighlighter { get; init; }
        public StylusTip StylusTip { get; init; }
    }
}
