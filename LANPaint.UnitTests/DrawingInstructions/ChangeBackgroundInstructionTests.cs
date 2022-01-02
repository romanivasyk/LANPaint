using System;
using System.Windows.Media;
using LANPaint.DrawingInstructions;
using LANPaint.DrawingInstructions.Interfaces;
using LANPaint.Model;
using Moq;
using Xunit;

namespace LANPaint.UnitTests.DrawingInstructions;

public class ChangeBackgroundInstructionTests
{
    [Fact]
    public void CtorArgbColor()
    {
        var color = new ARGBColor(100, 100, 100, 100);
        var instruction = new ChangeBackgroundInstruction(color);
        Assert.Equal(instruction.Background, color);
    }

    [Fact]
    public void CtorColor()
    {
        var color = new Color {A = 100, R = 100, G = 100, B = 100};
        var instruction = new ChangeBackgroundInstruction(color);
        Assert.Equal(color.A, instruction.Background.A);
        Assert.Equal(color.R, instruction.Background.R);
        Assert.Equal(color.G, instruction.Background.G);
        Assert.Equal(color.B, instruction.Background.B);
    }

    [Fact]
    public void ExecuteDrawingInstructionPassNull()
    {
        var color = new ARGBColor(100, 100, 100, 100);
        var instruction = new ChangeBackgroundInstruction(color);
        Assert.Throws<ArgumentNullException>(() => instruction.ExecuteDrawingInstruction(null));
    }

    [Fact]
    public void ExecuteDrawingInstruction()
    {
        var repositoryMock = new Mock<IDrawingInstructionRepository>();
        var color = new ARGBColor(100, 100, 100, 100);
        var instruction = new ChangeBackgroundInstruction(color);

        instruction.ExecuteDrawingInstruction(repositoryMock.Object);

        repositoryMock.Verify(mock => mock.ChangeBackground(instruction), Times.Once);
    }
}