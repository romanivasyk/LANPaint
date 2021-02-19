using System;
using System.Collections.Generic;
using System.Windows.Ink;
using System.Windows.Media;

namespace LANPaint.Model
{
    public interface IDrawingInstructionRepository
    {
        public void Clear();
        public void ChangeBackground(ChangeBackgroundInstruction instruction);
        public void Erase(EraseInstruction instruction);
        public void Draw(DrawInstruction instruction);
        public void ApplySnapshot(SnapshotInstruction instruction);
    }

    public interface IDrawingInstruction
    {
        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository);
    }

    [Serializable]
    public class ClearInstruction : IDrawingInstruction
    {
        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            instructionRepository.Clear();
        }
    }

    [Serializable]
    public class ChangeBackgroundInstruction : IDrawingInstruction
    {
        public ARGBColor Background { get; }

        public ChangeBackgroundInstruction(ARGBColor background)
        {
            Background = background;
        }

        public ChangeBackgroundInstruction(Color background) : this(ARGBColor.FromColor(background))
        { }

        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            instructionRepository.ChangeBackground(this);
        }
    }

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

    [Serializable]
    public class SnapshotInstruction : IDrawingInstruction
    {
        public ARGBColor Background { get; }
        public IEnumerable<SerializableStroke> Strokes { get; }

        public SnapshotInstruction(ARGBColor background, IEnumerable<SerializableStroke> strokes)
        {
            Background = background;
            Strokes = strokes;
        }

        public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
        {
            instructionRepository.ApplySnapshot(this);
        }
    }
}
