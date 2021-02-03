using LANPaint.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public class Chainer : UDPBroadcastDecorator
    {
        public int SegmentPayloadLength { get; }

        private readonly BinaryFormatter _formatter;
        private readonly Dictionary<Guid, SortedList<long, Segment>> _segmentBuffer;

        public Chainer(IUDPBroadcast udpBroadcaster, int segmentPayloadLength = 8192) : base(udpBroadcaster)
        {
            SegmentPayloadLength = segmentPayloadLength;
            _formatter = new BinaryFormatter();
            _segmentBuffer = new Dictionary<Guid, SortedList<long, Segment>>();
        }

        public override async Task<int> SendAsync(byte[] payload)
        {
            var sequenceGuid = Guid.NewGuid();
            var sequenceLength = payload.Length % SegmentPayloadLength > 0 ?
                                 payload.Length / SegmentPayloadLength + 1 :
                                 payload.Length / SegmentPayloadLength;

            for (var i = 0; i < sequenceLength; i++)
            {
                var beginWith = i * SegmentPayloadLength;

                var endBefore = i + 1 == sequenceLength ?
                                         payload.Length :
                                         beginWith + SegmentPayloadLength;

                var segment = new Segment(i, payload[beginWith..endBefore]);
                var packet = new Packet(sequenceGuid, sequenceLength, segment);

                var bytes = _formatter.OneLineSerialize(packet);
                await UdpBroadcast.SendAsync(bytes);
            }

            return payload.Length;
        }

        public override async Task<byte[]> ReceiveAsync(CancellationToken token = default)
        {
            while (true)
            {
                byte[] bytes;
                try
                {
                    bytes = await UdpBroadcast.ReceiveAsync(token);
                }
                catch
                {
                    _segmentBuffer.Clear();
                    throw;
                }

                var packet = _formatter.OneLineDeserialize<Packet>(bytes);

                if (_segmentBuffer.ContainsKey(packet.SequenceGuid))
                {
                    _segmentBuffer[packet.SequenceGuid].Add(packet.Segment.SequenceIndex, packet.Segment);

                    if (_segmentBuffer[packet.SequenceGuid].Count != packet.SequenceLength ||
                        _segmentBuffer[packet.SequenceGuid].Last().Key + 1 != packet.SequenceLength) continue;

                    var messageLength = _segmentBuffer[packet.SequenceGuid].Values.Sum(segment => segment.Payload.LongLength);
                    var message = new byte[messageLength];
                    var messageOffset = 0;
                    foreach (var segment in _segmentBuffer[packet.SequenceGuid].Values)
                    {
                        Buffer.BlockCopy(segment.Payload, 0, message, messageOffset, segment.Payload.Length);
                        messageOffset += segment.Payload.Length;
                    }
                    _segmentBuffer.Remove(packet.SequenceGuid);
                    return message;
                }
                else
                {
                    if (packet.SequenceLength == 1)
                    {
                        return packet.Segment.Payload;
                    }

                    var segments = new SortedList<long, Segment>
                    {
                        { packet.Segment.SequenceIndex, packet.Segment }
                    };
                    _segmentBuffer.Add(packet.SequenceGuid, segments);
                }
            }
        }

        public override ValueTask ClearBufferAsync() => UdpBroadcast.ClearBufferAsync();

        public override void Dispose() => UdpBroadcast?.Dispose();
    }
}
