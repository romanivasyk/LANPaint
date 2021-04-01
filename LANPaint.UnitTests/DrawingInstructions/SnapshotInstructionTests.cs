using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Ink;
using LANPaint.DrawingInstructions;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;
using Moq;
using Xunit;

namespace LANPaint.UnitTests.DrawingInstructions
{
    public class SnapshotInstructionTests
    {
        private readonly StrokeAttributes _attributes;
        private readonly Point[] _points;

        public SnapshotInstructionTests()
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
        }

        [Fact]
        public void Ctor()
        {
            var background = ARGBColor.Default;
            var strokes = new List<SerializableStroke> {new(_attributes, _points)};
            var instruction = new SnapshotInstruction(background, strokes);

            Assert.Equal(background, instruction.Background);
            Assert.True(strokes.SequenceEqual(instruction.Strokes));
        }

        [Fact]
        public void CtorPassEmptyStrokesCollection()
        {
            var background = ARGBColor.Default;
            var strokes = Enumerable.Empty<SerializableStroke>().ToArray();
            var instruction = new SnapshotInstruction(background, strokes);

            Assert.Equal(background, instruction.Background);
            Assert.False(instruction.Strokes.Any());
        }

        [Fact]
        public void CtorPassNullStrokesCollection()
        {
            var background = ARGBColor.Default;
            Assert.Throws<ArgumentNullException>(() => new SnapshotInstruction(background, null));
        }

        [Fact]
        public void ExecuteDrawingInstruction()
        {
            var background = ARGBColor.Default;
            var strokes = new List<SerializableStroke> {new(_attributes, _points)};
            var instruction = new SnapshotInstruction(background, strokes);
            var repositoryMock = new Mock<IDrawingInstructionRepository>();

            instruction.ExecuteDrawingInstruction(repositoryMock.Object);

            repositoryMock.Verify(mock => mock.ApplySnapshot(instruction), Times.Once);
        }

        [Fact]
        public void ExecuteDrawingInstructionPassNull()
        {
            var background = ARGBColor.Default;
            var strokes = new List<SerializableStroke> {new(_attributes, _points)};
            var instruction = new SnapshotInstruction(background, strokes);

            Assert.Throws<ArgumentNullException>(() => instruction.ExecuteDrawingInstruction(null));
        }
    }
}