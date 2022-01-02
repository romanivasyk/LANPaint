using System;
using System.Linq;
using LANPaint.Services.Broadcast;
using Xunit;

namespace LANPaint.UnitTests.Services.Broadcast;

public class DataReceivedEventArgsTests
{
    [Fact]
    public void Ctor_ValidData()
    {
        var data = new byte[] {5, 223, 0, 11, 183, 92};
        var dataReceivedArgs = new DataReceivedEventArgs(data);
        Assert.True(data.SequenceEqual(dataReceivedArgs.Data));
    }

    [Fact]
    public void Ctor_NullData()
    {
        byte[] data = null;
        Assert.Throws<ArgumentNullException>(() => new DataReceivedEventArgs(data));
    }

    [Fact]
    public void Ctor_EmptyDataArray()
    {
        var data = Array.Empty<byte>();
        Assert.Throws<ArgumentException>(() => new DataReceivedEventArgs(data));
    }
}