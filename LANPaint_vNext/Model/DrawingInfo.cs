using System;
using System.Windows.Media;
using System.Windows.Ink;


namespace LANPaint_vNext.Model
{
    public readonly struct DrawingInfo
    {
        public Color Background { get; }
        public bool IsEraser { get; }
        public bool ClearBoard { get; }
        public Stroke Stroke { get; }

        public DrawingInfo(Color background, Stroke stroke, 
                           bool isEraser = false, bool clearBoard = false)
        {
            Background = background;
            //Background = new ARGBColor(background.A, background.R, background.G, background.B);
            Stroke = stroke;
            IsEraser = isEraser;
            ClearBoard = clearBoard;
        }

        public override string ToString()
        {
            return $"Background: {Background}\nIsEraser: {IsEraser}\nClearBoard: {ClearBoard}\nStroke: {Stroke}";
        }

        //public struct ARGBColor
        //{
        //    public byte A { get; set; }
        //    public byte R { get; set; }
        //    public byte G { get; set; }
        //    public byte B { get; set; }

        //    public ARGBColor(byte a, byte r, byte g, byte b)
        //    {
        //        A = a;
        //        R = r;
        //        G = g;
        //        B = b;
        //    }
        //}
    }
}
