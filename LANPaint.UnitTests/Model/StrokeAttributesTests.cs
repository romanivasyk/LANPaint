using System.Windows.Ink;
using LANPaint.Model;
using Xunit;

namespace LANPaint.UnitTests.Model
{
    public class StrokeAttributesTests
    {
        private StrokeAttributes _attributes;

        public StrokeAttributesTests()
        {
            _attributes = new StrokeAttributes
            {
                Color = ARGBColor.Default,
                Height = 2,
                Width = 2,
                IgnorePressure = true,
                IsHighlighter = false,
                StylusTip = StylusTip.Ellipse
            };
        }

        [Fact]
        public void GetHashCodeForEqualAttributes()
        {
            var anotherAttributes = new StrokeAttributes
            {
                Color = _attributes.Color,
                Height = _attributes.Height,
                Width = _attributes.Width,
                IgnorePressure = _attributes.IgnorePressure,
                IsHighlighter = _attributes.IsHighlighter,
                StylusTip = _attributes.StylusTip
            };

            var hash = _attributes.GetHashCode();
            var anotherHash = anotherAttributes.GetHashCode();

            Assert.Equal(hash, anotherHash);
        }
        
        [Fact]
        public void GetHashCodeForNonEqualAttributes()
        {
            var anotherAttributes = new StrokeAttributes
            {
                Color = _attributes.Color,
                Height = _attributes.Height + 5,
                Width = _attributes.Width + 1,
                IgnorePressure = _attributes.IgnorePressure,
                IsHighlighter = !_attributes.IsHighlighter,
                StylusTip = _attributes.StylusTip
            };

            var hash = _attributes.GetHashCode();
            var anotherHash = anotherAttributes.GetHashCode();

            Assert.NotEqual(hash, anotherHash);
        }
    }
}