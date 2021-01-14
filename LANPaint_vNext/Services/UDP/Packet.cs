using System;

namespace LANPaint_vNext.Services.UDP
{
    [Serializable]
    public readonly struct Packet
    {
        public Guid SequenceGUID { get; }
        public long SequenceLength { get; }
        public Segment Segment { get; }

        public Packet(Guid sequenceGuid, long sequenceLength, Segment segment)
        {
            SequenceGUID = sequenceGuid;
            SequenceLength = sequenceLength;
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
            SequenceIndex = sequenceIndex;
            Payload = payload;
        }
    }
}
