using System;
using System.Linq;
using LANPaint.Services.Broadcast.Decorators;
using Xunit;

namespace LANPaint.UnitTests.Services.Broadcast.Chainer
{
    public class SegmentTests
    {
        [Fact]
        public void Ctor_ValidData()
        {
            const int index = 5;
            var data = new byte[] {50, 34, 124, 58, 255, 255, 0, 94, 183};

            var segment = new Segment(index, data);

            Assert.Equal(index, segment.SequenceIndex);
            Assert.True(data.SequenceEqual(segment.Payload));
        }

        [Fact]
        public void Ctor_PassNullData()
        {
            const int index = 5;
            Assert.Throws<ArgumentNullException>(() => new Segment(index, null));
        }

        [InlineData(long.MinValue)]
        [InlineData(-1)]
        [Theory]
        public void Ctor_PassNegativeIndex(long index)
        {
            var data = new byte[] {50, 34, 124, 58, 255, 255, 0, 94, 183};
            Assert.Throws<ArgumentOutOfRangeException>(() => new Segment(index, data));
        }

        [Fact]
        public void Ctor_PassZeroIndex()
        {
            const int index = 0;
            var data = new byte[] {50, 34, 124, 58, 255, 255, 0, 94, 183};

            var segment = new Segment(index, data);

            Assert.Equal(index, segment.SequenceIndex);
            Assert.True(data.SequenceEqual(segment.Payload));
        }
    }
}