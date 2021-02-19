using System;
using LANPaint.DrawingInstructions.Interfaces;

namespace LANPaint.DrawingInstructions
{
    [Serializable]
    public class ClearInstruction : IDrawingInstruction
    {
        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            instructionRepository.Clear();
        }
    }
}