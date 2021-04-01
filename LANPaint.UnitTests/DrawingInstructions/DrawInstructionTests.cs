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
    public class DrawInstructionTests
    {
        private readonly StrokeAttributes _attributes;
        private readonly Point[] _points;
        private readonly Stroke _nonSerializableStroke;

        public DrawInstructionTests()
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
        public void CtorSerializableStroke_ValidData()
        {
            var stroke = new SerializableStroke(_attributes, _points);
            var instruction = new DrawInstruction(stroke);
            Assert.Equal(stroke, instruction.DrawingStroke);
        }

        [Fact]
        public void CtorStroke_ValidData()
        {
            var instruction = new DrawInstruction(_nonSerializableStroke);

            Assert.True(instruction.DrawingStroke.Points.SequenceEqual(_points));
            Assert.Equal(instruction.DrawingStroke.Attributes.Color, _attributes.Color);
            Assert.Equal(instruction.DrawingStroke.Attributes.Height, _attributes.Height);
            Assert.Equal(instruction.DrawingStroke.Attributes.Width, _attributes.Width);
            Assert.Equal(instruction.DrawingStroke.Attributes.IgnorePressure, _attributes.IgnorePressure);
            Assert.Equal(instruction.DrawingStroke.Attributes.IsHighlighter, _attributes.IsHighlighter);
            Assert.Equal(instruction.DrawingStroke.Attributes.StylusTip, _attributes.StylusTip);
        }

        [Fact]
        public void CtorStroke_PassNull()
        {
            Assert.Throws<ArgumentNullException>(() => new DrawInstruction(null));
        }

        [Fact]
        public void ExecuteDrawingInstruction()
        {
            var repositoryMock = new Mock<IDrawingInstructionRepository>();
            var stroke = new SerializableStroke(_attributes, _points);
            var instruction = new DrawInstruction(stroke);

            instruction.ExecuteDrawingInstruction(repositoryMock.Object);

            repositoryMock.Verify(instructionRepository => instructionRepository.Draw(instruction), Times.Once);
        }

        [Fact]
        public void ExecuteDrawingInstruction_PassNull()
        {
            var stroke = new SerializableStroke(_attributes, _points);
            var instruction = new DrawInstruction(stroke);

            Assert.Throws<ArgumentNullException>(() => instruction.ExecuteDrawingInstruction(null));
        }
    }
}