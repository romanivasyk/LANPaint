#if false
using System;
using System.Collections.Generic;

namespace LANPaint.Model
{
    [Serializable]
    public class BoardSnapshot
    {
        public ARGBColor Background { get; }
        public IEnumerable<SerializableStroke> Strokes { get; }

        public BoardSnapshot(ARGBColor background, IEnumerable<SerializableStroke> strokes)
        {
            Background = background;
            Strokes = strokes;
        }
    }
}
#endif