using System.Windows.Media;
using LANPaint.Model;
using Xunit;

namespace LANPaint.UnitTests.Model
{
    public class ARGBColorTests
    {
        [InlineData(100, 101, 102, 103)]
        [InlineData(255, 255, 0, 100)]
        [InlineData(0, 0, 0, 0)]
        [Theory]
        public void Ctor_ValidDataTheory(byte a, byte r, byte g, byte b)
        {
            var color = new ARGBColor(a, r, g, b);

            Assert.Equal(a, color.A);
            Assert.Equal(r, color.R);
            Assert.Equal(g, color.G);
            Assert.Equal(b, color.B);
        }

        [Fact]
        public void BlackIsDefault()
        {
            var defaultColor = ARGBColor.Default;
            var black = Colors.Black;

            Assert.Equal(black.A, defaultColor.A);
            Assert.Equal(black.R, defaultColor.R);
            Assert.Equal(black.G, defaultColor.G);
            Assert.Equal(black.B, defaultColor.B);
        }

        [Fact]
        public void AsColor()
        {
            var yellow = Colors.Yellow;
            var color = new ARGBColor() {A = yellow.A, R = yellow.R, G = yellow.G, B = yellow.B};

            var result = color.AsColor();

            Assert.Equal(yellow, result);
        }

        [Fact]
        public void FromColor()
        {
            var yellow = Colors.Yellow;
            var result = ARGBColor.FromColor(yellow);

            Assert.Equal(yellow.A, result.A);
            Assert.Equal(yellow.R, result.R);
            Assert.Equal(yellow.G, result.G);
            Assert.Equal(yellow.B, result.B);
        }

        [Fact]
        public void EqualityEquals()
        {
            var color = new ARGBColor(78, 123, 255, 18);
            var anotherColor = new ARGBColor(78, 123, 255, 18);

            var result = color.Equals(anotherColor);

            Assert.True(result);
        }

        [Fact]
        public void OverriddenEquals()
        {
            var color = new ARGBColor(78, 123, 255, 18);
            var anotherColor = (object) new ARGBColor(78, 123, 255, 18);

            var result = color.Equals(anotherColor);

            Assert.True(result);
        }

        [Fact]
        public void EqualityOperator()
        {
            var color = new ARGBColor(78, 123, 255, 18);
            var anotherColor = new ARGBColor(78, 123, 255, 18);

            var result = color == anotherColor;

            Assert.True(result);
        }

        [Fact]
        public void NonEqualityOperator()
        {
            var color = new ARGBColor(78, 123, 255, 18);
            var anotherColor = new ARGBColor(78, 123, 255, 18);

            var result = color != anotherColor;

            Assert.False(result);
        }

        [Fact]
        public void GetHashCodeForEqualColors()
        {
            var color = new ARGBColor(78, 123, 255, 18);
            var anotherColor = new ARGBColor(78, 123, 255, 18);

            var hashCode = color.GetHashCode();
            var anotherHashCode = anotherColor.GetHashCode();

            Assert.Equal(hashCode, anotherHashCode);
        }

        [Fact]
        public void GetHashCodeForNonEqualColors()
        {
            var color = new ARGBColor(78, 123, 255, 18);
            var anotherColor = new ARGBColor(53, 188, 0, 13);

            var hashCode = color.GetHashCode();
            var anotherHashCode = anotherColor.GetHashCode();

            Assert.NotEqual(hashCode, anotherHashCode);
        }

        [Fact]
        public void ToStringTest()
        {
            var color = new ARGBColor(78, 123, 255, 18);
            var expected = $"A:{color.A}, R:{color.R}, G:{color.G}, B:{color.B}";

            var result = color.ToString();

            Assert.Equal(expected, result);
        }
    }
}