using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LANPaint.Services.Broadcast;
using Moq;
using Xunit;

namespace LANPaint.UnitTests.Services.Broadcast.Chainer;

public class ChainerTest
{
    private readonly Mock<IBroadcast> _broadcastImplMock;
    private static readonly Random Random = new Random();

    public ChainerTest()
    {
        _broadcastImplMock = new Mock<IBroadcast>();
        _broadcastImplMock.Setup(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()))
            .ReturnsAsync((byte[] dataParam) => dataParam.Length);
    }

    [Fact]
    public void Ctor_PassNullBroadcastImpl()
    {
        Assert.Throws<ArgumentNullException>(() => new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(null));
    }

    [Fact]
    public void Ctor_LowerThanAllowedSegmentLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, LANPaint.Services.Broadcast.UDP.Chainer.Chainer.MinSegmentLength - 1));
    }

    [Fact]
    public void Ctor_HigherThanAllowedSegmentLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, LANPaint.Services.Broadcast.UDP.Chainer.Chainer.MaxSegmentLength + 1));
    }

    [Fact]
    public void Ctor_ValidSegmentLength()
    {
        var _ = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object,
            LANPaint.Services.Broadcast.UDP.Chainer.Chainer.MinSegmentLength + (LANPaint.Services.Broadcast.UDP.Chainer.Chainer.MaxSegmentLength - LANPaint.Services.Broadcast.UDP.Chainer.Chainer.MinSegmentLength) / 2);
    }

    [Fact]
    public void LocalEndPoint_CheckInnerCall()
    {
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object);
        var _ = chainer.LocalEndPoint;
        _broadcastImplMock.Verify(broadcast => broadcast.LocalEndPoint, Times.Once);
    }

    //Does it even make sense to check this?
    [Fact]
    public async void SendAsync_CheckReturnValue()
    {
        const int dataLength = 10000;
        var data = new byte[dataLength];
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object);

        var result = await chainer.SendAsync(data);

        //We know that Chainer wraps data into Package, so sent amount of bytes
        //definitely will be more than initial payload size.
        Assert.True(result > dataLength);
    }

    [Fact]
    public async void SendReceiveAsync_CheckSendingReceivingOfTheSameData()
    {
        var storage = new Stack<byte[]>();
        var broadcastImplMock = new Mock<IBroadcast>();
        broadcastImplMock.Setup(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()))
            .Callback((byte[] dataParam) => storage.Push(dataParam))
            .ReturnsAsync((byte[] dataParam) => dataParam.Length);
        broadcastImplMock.Setup(broadcast => broadcast.ReceiveAsync(CancellationToken.None))
            .ReturnsAsync(() => storage.Pop());
        var broadcastImpl = broadcastImplMock.Object;
        var data = GenerateRandomByteSequence(24000);

        await broadcastImpl.SendAsync(data);
        var result = await broadcastImpl.ReceiveAsync(CancellationToken.None);

        Assert.True(result.SequenceEqual(data));
    }

    [Theory]
    [InlineData(10000, 8192, 2)]
    [InlineData(10000, 10000, 1)]
    [InlineData(20100, 10000, 3)]
    [InlineData(0, 8192, 0)]
    public async void SendAsync_CheckNumberOfSendCalls(int dataLength, int maxSegmentLength,
        int expectedSendCallsNumber)
    {
        var data = new byte[dataLength];
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, maxSegmentLength);
        await chainer.SendAsync(data);
        _broadcastImplMock.Verify(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()),
            Times.Exactly(expectedSendCallsNumber));
    }

    [Fact]
    public async void SendAsync_PassingNullData()
    {
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object);
        await Assert.ThrowsAsync<ArgumentNullException>(() => chainer.SendAsync(null));
    }

    [Fact]
    public async void ReceiveAsync_ReceiveSegmented()
    {
        const int payloadLength = 20000;
        const int packetLength = 5000;
        var data = GenerateRandomByteSequence(payloadLength);
        var chainStorage = new List<byte[]>();
        _broadcastImplMock.Setup(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()))
            .ReturnsAsync((byte[] dataParam) =>
            {
                chainStorage.Add(dataParam);
                return dataParam.Length;
            });

        //It used just to fill chainStorage with payloadLength/packetLength packets with bytes to use it for receive
        var chainerFiller = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, packetLength);
        await chainerFiller.SendAsync(data);
        using var enumerator = chainStorage.GetEnumerator();
        _broadcastImplMock.Setup(broadcast => broadcast.ReceiveAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                enumerator.MoveNext();
                return Task.FromResult(enumerator.Current);
            });
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, 5000);

        var result = await chainer.ReceiveAsync();

        Assert.True(data.SequenceEqual(result));
    }

    [Fact]
    public async void ReceiveAsync_ReceiveSegmentedOfTwoSequences()
    {
        const int payloadLength = 20000;
        const int packetLength = 5000;
        var data = GenerateRandomByteSequence(payloadLength);
        var anotherData = GenerateRandomByteSequence(packetLength * 2);

        var chainStorage = new List<byte[]>();
        _broadcastImplMock.Setup(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()))
            .ReturnsAsync((byte[] dataParam) =>
            {
                chainStorage.Add(dataParam);
                return dataParam.Length;
            });

        //It used just to fill chainStorage with payloadLength/packetLength packets with bytes to use it for receive
        var chainerFiller = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, packetLength);
        await chainerFiller.SendAsync(anotherData);
        await chainerFiller.SendAsync(data);

        //Remove segment to make one of the sequences unable to recompose
        chainStorage.RemoveAt(0);

        using var enumerator = chainStorage.GetEnumerator();
        _broadcastImplMock.Setup(broadcast => broadcast.ReceiveAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                enumerator.MoveNext();
                return Task.FromResult(enumerator.Current);
            });
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, 5000);

        var result = await chainer.ReceiveAsync();

        Assert.True(data.SequenceEqual(result));
    }

    [Fact]
    public async void ReceiveAsync_CancelAfterFirstReceive()
    {
        const int payloadLength = 20000;
        const int packetLength = 5000;
        var data = GenerateRandomByteSequence(payloadLength);
        var chainStorage = new List<byte[]>();
        _broadcastImplMock.Setup(broadcast => broadcast.SendAsync(It.IsAny<byte[]>()))
            .ReturnsAsync((byte[] dataParam) =>
            {
                chainStorage.Add(dataParam);
                return dataParam.Length;
            });

        //It used just to fill chainStorage with payloadLength/packetLength packets with bytes to use it for receive
        var chainerFiller = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, packetLength);
        await chainerFiller.SendAsync(data);

        var tokenSource = new CancellationTokenSource();
        var isFirstIterationDone = false;
        _broadcastImplMock.SetupSequence(broadcast => broadcast.ReceiveAsync(It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                isFirstIterationDone = true;
                return Task.FromResult(chainStorage[0]);
            })
            .Returns(
                () =>
                {
                    tokenSource.Cancel();
                    return Task.FromResult(chainStorage[1]);
                })
            .Returns(() => throw new Exception());
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object, 5000);

        await Assert.ThrowsAsync<OperationCanceledException>(() => chainer.ReceiveAsync(tokenSource.Token));
        Assert.True(isFirstIterationDone);
    }

    [Fact]
    public async void ReceiveAsync_CancelReceiveAtStart()
    {
        var tokenSource = new CancellationTokenSource();
        tokenSource.Cancel();
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object);

        await Assert.ThrowsAsync<OperationCanceledException>(() => chainer.ReceiveAsync(tokenSource.Token));
    }

    [Fact]
    public async void ClearBufferAsync_CheckInnerCall()
    {
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object);
        await chainer.ClearBufferAsync();
        _broadcastImplMock.Verify(broadcast => broadcast.ClearBufferAsync(), Times.Once);
    }

    [Fact]
    public void Dispose_CheckInnerCall()
    {
        var chainer = new LANPaint.Services.Broadcast.UDP.Chainer.Chainer(_broadcastImplMock.Object);
        chainer.Dispose();
        _broadcastImplMock.Verify(broadcast => broadcast.Dispose(), Times.Once);
    }

    private static byte[] GenerateRandomByteSequence(int length)
    {
        var sequence = new byte[length];

        for (var i = 0; i < length; i++)
        {
            sequence[i] = (byte) Random.Next(256);
        }

        return sequence;
    }
}