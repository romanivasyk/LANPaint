using System;
using LANPaint.DrawingInstructions;
using LANPaint.DrawingInstructions.Interfaces;
using Moq;
using Xunit;

namespace LANPaint.UnitTests.DrawingInstructions
{
    public class ClearInstructionTests
    {
        [Fact]
        public void ExecuteDrawingInstructionPassNull()
        {
            var instruction = new ClearInstruction();
            Assert.Throws<ArgumentNullException>(() => instruction.ExecuteDrawingInstruction(null));
        }

        [Fact]
        public void ExecuteDrawingInstruction()
        {
            var repositoryMock = new Mock<IDrawingInstructionRepository>();
            var instruction = new ClearInstruction();
            
            instruction.ExecuteDrawingInstruction(repositoryMock.Object);
            
            repositoryMock.Verify(mock=>mock.Clear(), Times.Once);
        }
    }
}