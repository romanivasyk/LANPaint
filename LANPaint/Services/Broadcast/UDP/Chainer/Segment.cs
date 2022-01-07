using System;

namespace LANPaint.Services.Broadcast.UDP.Chainer;

[Serializable]
public readonly struct Segment : IComparable<Segment>
{
    public long SequenceIndex { get; }
    public byte[] Payload { get; }

    public Segment(long sequenceIndex, byte[] payload)
    {
        if (sequenceIndex < 0) throw new ArgumentOutOfRangeException(nameof(sequenceIndex));
        SequenceIndex = sequenceIndex;
        Payload = payload ?? throw new ArgumentNullException(nameof(payload));
    }

    public int CompareTo(Segment other) => SequenceIndex.CompareTo(other.SequenceIndex);
}