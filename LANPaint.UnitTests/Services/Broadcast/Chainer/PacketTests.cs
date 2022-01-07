using System;
using LANPaint.Services.Broadcast.Decorators;
using LANPaint.Services.Broadcast.UDP.Chainer;
using Xunit;

namespace LANPaint.UnitTests.Services.Broadcast.Chainer;

public class PacketTests
{
    private const int DefaultSequenceLength = 1;

    [Fact]
    public void Ctor_ValidData()
    {
        var guid = Guid.NewGuid();
        var segment = new Segment(0, new byte[] {10, 0, 155, 240});

        var packet = new Package(guid, DefaultSequenceLength, segment);

        Assert.Equal(guid, packet.SequenceGuid);
        Assert.Equal(DefaultSequenceLength, packet.SequenceLength);
        Assert.Equal(segment, packet.Segment);
    }

    [Fact]
    public void Ctor_EmptyGuid()
    {
        var guid = Guid.Empty;
        var segment = new Segment(0, new byte[] {10, 0, 155, 240});

        Assert.Throws<ArgumentException>(() => new Package(guid, DefaultSequenceLength, segment));
    }

    [InlineData(long.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    [Theory]
    public void Ctor_InvalidSequenceLength(long sequenceLength)
    {
        var guid = Guid.NewGuid();
        var segment = new Segment(0, new byte[] {10, 0, 155, 240});

        Assert.Throws<ArgumentOutOfRangeException>(() => new Package(guid, sequenceLength, segment));
    }

    [InlineData(5)]
    [InlineData(10)]
    [Theory]
    public void Ctor_PassSegmentWithOutOfRangeIndex(long sequenceIndex)
    {
        var guid = Guid.NewGuid();
        const int sequenceLength = 5;
        var data = new byte[] {50, 34, 124, 58, 255, 255, 0, 94, 183};
        var segment = new Segment(sequenceIndex, data);

        Assert.Throws<ArgumentException>(() => new Package(guid, sequenceLength, segment));
    }
}