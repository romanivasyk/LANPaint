using System;
using System.Collections.Generic;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;

namespace LANPaint.DrawingInstructions;

[Serializable]
public class SnapshotInstruction : IDrawingInstruction
{
    public ARGBColor Background { get; }
    public IEnumerable<SerializableStroke> Strokes { get; }

    public SnapshotInstruction(ARGBColor background, IEnumerable<SerializableStroke> strokes)
    {
        Background = background;
        Strokes = strokes ?? throw new ArgumentNullException(nameof(strokes));
    }

    public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
    {
        if (instructionRepository == null) throw new ArgumentNullException(nameof(instructionRepository));
        instructionRepository.ApplySnapshot(this);
    }
}