namespace LANPaint.DrawingInstructions.Interfaces;

public interface IDrawingInstructionRepository
{
    public void Clear();
    public void ChangeBackground(ChangeBackgroundInstruction instruction);
    public void Erase(EraseInstruction instruction);
    public void Draw(DrawInstruction instruction);
    public void ApplySnapshot(SnapshotInstruction instruction);
}