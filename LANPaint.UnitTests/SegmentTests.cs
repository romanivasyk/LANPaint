using System;
using System.Linq;
using LANPaint.Services.Broadcast.Decorators;
using Xunit;

namespace LANPaint.UnitTests
{
    public class SegmentTests
    {
        [Fact]
        public void Ctor_ValidData()
        {
            var index = 5;
            var data = new byte[] {50, 34, 124, 58, 255, 255, 0, 94, 183};

            var segment = new Segment(index, data);

            Assert.Equal(index, segment.SequenceIndex);
            Assert.True(data.SequenceEqual(segment.Payload));
        }

        [Fact]
        public void Ctor_PassNullData()
        {
            var index = 5;
            byte[] data = null;

            Assert.Throws<ArgumentNullException>(() => new Segment(index, data));
        }

        [InlineData(int.MinValue)]
        [InlineData(-1)]
        [Theory]
        public void Ctor_PassNegativeIndex(int index)
        {
            var data = new byte[] {50, 34, 124, 58, 255, 255, 0, 94, 183};
            Assert.Throws<ArgumentOutOfRangeException>(() => new Segment(index, data));
        }

        [Fact]
        public void Ctor_PassZeroIndex()
        {
            var index = 0;
            var data = new byte[] {50, 34, 124, 58, 255, 255, 0, 94, 183};

            var segment = new Segment(index, data);

            Assert.Equal(index, segment.SequenceIndex);
            Assert.True(data.SequenceEqual(segment.Payload));
        }
    }
}