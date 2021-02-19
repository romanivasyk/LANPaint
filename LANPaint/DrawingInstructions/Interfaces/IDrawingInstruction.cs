namespace LANPaint.DrawingInstructions.Interfaces
{
    public interface IDrawingInstruction
    {
        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository);
    }
}