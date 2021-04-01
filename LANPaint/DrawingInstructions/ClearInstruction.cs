using System;
using LANPaint.DrawingInstructions.Interfaces;

namespace LANPaint.DrawingInstructions
{
    [Serializable]
    public class ClearInstruction : IDrawingInstruction
    {
        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            if (instructionRepository == null) throw new ArgumentNullException(nameof(instructionRepository));
            instructionRepository.Clear();
        }
    }
}