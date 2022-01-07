using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using LANPaint.Extensions;
using LANPaint.Services.Broadcast.Decorators;

namespace LANPaint.Services.Broadcast.UDP.Chainer;

public class Chainer : BroadcastDecorator
{
    public const int MinSegmentLength = 1024;
    public const int MaxSegmentLength = 65535;
    public const int CleanupPeriodMs = 60000;

    private readonly Timer _cleanupTimer;
    private readonly BinaryFormatter _formatter;
    private readonly int _payloadSegmentLength;
    private readonly ConcurrentDictionary<Guid, Chain> _segmentBuffer;

    public Chainer(IBroadcast broadcast, int payloadSegmentLength = 8192) : base(broadcast)
    {
        if (payloadSegmentLength < MinSegmentLength || payloadSegmentLength > MaxSegmentLength)
            throw new ArgumentOutOfRangeException(nameof(payloadSegmentLength),
                $"Provided segment length should be in range from {MinSegmentLength} to {MaxSegmentLength}");

        _payloadSegmentLength = payloadSegmentLength;
        _formatter = new BinaryFormatter();
        _segmentBuffer = new ConcurrentDictionary<Guid, Chain>();
        _cleanupTimer = new Timer(CleanupCallback, null, 0, CleanupPeriodMs);
    }

    public override async Task<int> SendAsync(byte[] payload)
    {
        if (payload == null) throw new ArgumentNullException(nameof(payload));

        var sequenceGuid = Guid.NewGuid();
        var byteChunks = payload.Chunk(_payloadSegmentLength).ToArray();
        var chunksCount = byteChunks.GetLength(0);

        var bytesSentCount = 0;
        for (var i = 0; i < chunksCount; i++)
        {
            var segment = new Segment(i, byteChunks[i]);
            var package = new Package(sequenceGuid, chunksCount, segment);

            var bytes = _formatter.OneLineSerialize(package);
            bytesSentCount += await Broadcast.SendAsync(bytes);
        }

        return bytesSentCount;
    }

    public override async Task<byte[]> ReceiveAsync(CancellationToken token = default)
    {
        while (true)
        {
            byte[] bytes;
            try
            {
                token.ThrowIfCancellationRequested();
                bytes = await Broadcast.ReceiveAsync(token);
            }
            catch
            {
                _segmentBuffer.Clear();
                throw;
            }

            Package package;
            try
            {
                package = (Package)_formatter.OneLineDeserialize(bytes);
            }
            catch (SerializationException)
            {
                continue;
            }


            if (_segmentBuffer.ContainsKey(package.SequenceGuid))
            {
                _segmentBuffer[package.SequenceGuid].AddSegment(package.Segment);

                if (!_segmentBuffer[package.SequenceGuid].TryAssemble(out var assembledBytes)) continue;

                _segmentBuffer.Remove(package.SequenceGuid, out _);
                return assembledBytes;
            }

            if (package.SequenceLength == 1) return package.Segment.Payload;

            var chain = new Chain(package.SequenceLength);
            chain.AddSegment(package.Segment);

            _segmentBuffer.TryAdd(package.SequenceGuid, chain);
        }
    }

    private void CleanupCallback(object state)
    {
        var cleanupTime = DateTime.Now;
        foreach (var bufferKey in _segmentBuffer.Keys)
        {
            var timeSinceLastChainUpdate = cleanupTime - _segmentBuffer[bufferKey].LastAddedAt;
            if(timeSinceLastChainUpdate.Milliseconds < CleanupPeriodMs) continue;

            _segmentBuffer.Remove(bufferKey, out _);
        }
    }

    public override ValueTask ClearBufferAsync()
    {
        return Broadcast.ClearBufferAsync();
    }

    public override void Dispose()
    {
        _cleanupTimer.Dispose();
        base.Dispose();
    }
}