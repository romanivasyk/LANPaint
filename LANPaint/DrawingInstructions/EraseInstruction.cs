using System;
using System.Windows.Ink;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;

namespace LANPaint.DrawingInstructions
{
    [Serializable]
    public class EraseInstruction : IDrawingInstruction
    {
        public SerializableStroke ErasingStroke { get; }

        public EraseInstruction(SerializableStroke erasingStroke)
        {
            ErasingStroke = erasingStroke;
        }

        public EraseInstruction(Stroke stroke) : this(SerializableStroke.FromStroke(stroke))
        { }

        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            instructionRepository.Erase(this);
        }
    }
}