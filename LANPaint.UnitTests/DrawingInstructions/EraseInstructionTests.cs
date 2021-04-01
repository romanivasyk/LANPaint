using System;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using LANPaint.DrawingInstructions;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;
using Moq;
using Xunit;

namespace LANPaint.UnitTests.DrawingInstructions
{
    public class EraseInstructionTests
    {
        private readonly StrokeAttributes _attributes;
        private readonly Point[] _points;
        private readonly Stroke _nonSerializableStroke;

        public EraseInstructionTests()
        {
            _attributes = new StrokeAttributes()
            {
                Color = ARGBColor.Default,
                Height = 2,
                Width = 2,
                IgnorePressure = true,
                IsHighlighter = false,
                StylusTip = StylusTip.Ellipse
            };
            _points = Enumerable.Range(10, 100).Select(i => new Point(i, i)).ToArray();
            _nonSerializableStroke = new Stroke(new StylusPointCollection(_points),
                new DrawingAttributes()
                {
                    Color = _attributes.Color.AsColor(),
                    Height = _attributes.Height,
                    Width = _attributes.Width,
                    IgnorePressure = _attributes.IgnorePressure,
                    IsHighlighter = _attributes.IsHighlighter,
                    StylusTip = _attributes.StylusTip
                });
        }

        [Fact]
        public void CtorSerializableStroke()
        {
            var stroke = new SerializableStroke(_attributes, _points);
            var instruction = new EraseInstruction(stroke);

            Assert.Equal(stroke, instruction.ErasingStroke);
        }

        [Fact]
        public void CtorStroke()
        {
            var instruction = new EraseInstruction(_nonSerializableStroke);
            
            Assert.True(instruction.ErasingStroke.Points.SequenceEqual(_points));
            Assert.Equal(instruction.ErasingStroke.Attributes.Color, _attributes.Color);
            Assert.Equal(instruction.ErasingStroke.Attributes.Height, _attributes.Height);
            Assert.Equal(instruction.ErasingStroke.Attributes.Width, _attributes.Width);
            Assert.Equal(instruction.ErasingStroke.Attributes.IgnorePressure, _attributes.IgnorePressure);
            Assert.Equal(instruction.ErasingStroke.Attributes.IsHighlighter, _attributes.IsHighlighter);
            Assert.Equal(instruction.ErasingStroke.Attributes.StylusTip, _attributes.StylusTip);
        }
        
        [Fact]
        public void CtorStroke_PassNull()
        {
            Assert.Throws<ArgumentNullException>(() => new EraseInstruction(null));
        }
        
        [Fact]
        public void ExecuteDrawingInstruction()
        {
            var repositoryMock = new Mock<IDrawingInstructionRepository>();
            var stroke = new SerializableStroke(_attributes, _points);
            var instruction = new EraseInstruction(stroke);

            instruction.ExecuteDrawingInstruction(repositoryMock.Object);

            repositoryMock.Verify(instructionRepository => instructionRepository.Erase(instruction), Times.Once);
        }
        
        [Fact]
        public void ExecuteDrawingInstruction_PassNull()
        {
            var stroke = new SerializableStroke(_attributes, _points);
            var instruction = new EraseInstruction(stroke);

            Assert.Throws<ArgumentNullException>(() => instruction.ExecuteDrawingInstruction(null));
        }
    }
}