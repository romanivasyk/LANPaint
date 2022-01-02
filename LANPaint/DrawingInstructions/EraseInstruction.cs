using System;
using System.Windows.Ink;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;

namespace LANPaint.DrawingInstructions;

[Serializable]
public class EraseInstruction : IDrawingInstruction
{
    public SerializableStroke ErasingStroke { get; }

    public EraseInstruction(SerializableStroke stroke) => ErasingStroke = stroke;

    public EraseInstruction(Stroke stroke)
    {
        if (stroke == null) throw new ArgumentNullException(nameof(stroke));
        ErasingStroke = SerializableStroke.FromStroke(stroke);
    }

    public void ExecuteDrawingInstruction(IDrawingInstructionRepository instructionRepository)
    {
        if (instructionRepository == null) throw new ArgumentNullException(nameof(instructionRepository));
        instructionRepository.Erase(this);
    }
}