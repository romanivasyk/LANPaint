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

        public DrawInstruction(Stroke stroke)
        {
            if (stroke == null) throw new ArgumentNullException(nameof(stroke));
            SerializableStroke.FromStroke(stroke);
        }

        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            if (instructionRepository == null) throw new ArgumentNullException(nameof(instructionRepository));
            instructionRepository.Draw(this);
        }
    }
}