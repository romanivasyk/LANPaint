using System;
using System.Windows.Ink;

namespace LANPaint.Model
{
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
