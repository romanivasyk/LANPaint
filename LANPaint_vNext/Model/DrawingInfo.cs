using System;
using System.Windows.Media;
using System.Windows.Ink;


namespace LANPaint_vNext.Model
{
    [Serializable]
    public readonly struct DrawingInfo
    {
        public ARGBColor Background { get; }
        public bool IsEraser { get; }
        public bool ClearBoard { get; }
        public Stroke Stroke { get; }

        public DrawingInfo(Color background, Stroke stroke,
                           bool isEraser = false, bool clearBoard = false)
        {
            Background = new ARGBColor(background.A, background.R, background.G, background.B);
            Stroke = stroke;
            IsEraser = isEraser;
            ClearBoard = clearBoard;
        }

        public override string ToString()
        {
#warning TODO: Add Stroke representation
            return $"Background:{Background}, IsEraser:{IsEraser}, ClearBoard:{ClearBoard}";
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
    }
}
