using System;
using System.Collections.Generic;
using System.Text;

namespace LANPaint_vNext.Services.UDP
{
    [Serializable]
    public struct Packet
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
    public struct Segment
    {
        public long Index { get; }
        public byte[] Payload { get; }

        public Segment(long index, byte[] payload)
        {
            this.Index = index;
            Payload = payload;
        }
    }
}
