using System;

namespace LANPaint.Services.Broadcast.Decorators;

[Serializable]
public readonly struct Packet
{
    public Guid SequenceGuid { get; }
    public long SequenceLength { get; }
    public Segment Segment { get; }

    public Packet(Guid sequenceGuid, long sequenceLength, Segment segment)
    {
        SequenceGuid = sequenceGuid == Guid.Empty
            ? throw new ArgumentException("Sequence Guid cannot be empty!", nameof(sequenceGuid))
            : sequenceGuid;
        SequenceLength = sequenceLength <= 0 ? throw new ArgumentOutOfRangeException(nameof(sequenceLength)) : sequenceLength;

        if (segment.SequenceIndex >= sequenceLength)
            throw new ArgumentException(
                "Packet cannot contain segment which index in the packet sequence is equal or more of sequence length!",
                nameof(segment));
        Segment = segment;
    }
}

[Serializable]
public readonly struct Segment
{
    public long SequenceIndex { get; }
    public byte[] Payload { get; }

    public Segment(long sequenceIndex, byte[] payload)
    {
        if (sequenceIndex < 0) throw new ArgumentOutOfRangeException(nameof(sequenceIndex));
        SequenceIndex = sequenceIndex;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }
}