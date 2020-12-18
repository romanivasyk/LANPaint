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
            Stroke = stroke;
            IsEraser = isEraser;
            ClearBoard = clearBoard;
        }

        public override string ToString()
        {
            return $"Background: {Background}\nIsEraser: {IsEraser}\nClearBoard: {ClearBoard}\nStroke: {Stroke}";
        }
    }
}
