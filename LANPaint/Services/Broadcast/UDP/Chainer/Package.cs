using System;

namespace LANPaint.Services.Broadcast.UDP.Chainer;

[Serializable]
public readonly struct Package
{
    public Guid SequenceGuid { get; }
    public long SequenceLength { get; }
    public Segment Segment { get; }

    public Package(Guid sequenceGuid, long sequenceLength, Segment segment)
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