using System;
using System.Windows.Ink;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;

namespace LANPaint.DrawingInstructions
{
    [Serializable]
    public class DrawInstruction : IDrawingInstruction
    {
        public SerializableStroke DrawingStroke { get; }

        public DrawInstruction(SerializableStroke drawingStroke)
        {
            DrawingStroke = drawingStroke;
        }

        public DrawInstruction(Stroke stroke) : this(SerializableStroke.FromStroke(stroke))
        { }

        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            instructionRepository.Draw(this);
        }
    }
}