using System;
using System.Windows.Media;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;

namespace LANPaint.DrawingInstructions
{
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
            if (instructionRepository == null) throw new ArgumentNullException(nameof(instructionRepository));
            instructionRepository.ChangeBackground(this);
        }
    }
}