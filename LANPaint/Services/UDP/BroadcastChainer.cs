using LANPaint.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace LANPaint.Services.UDP
{
    public class BroadcastChainer : INetworkBroadcaster
    {
        public int SegmentPayloadLength { get; }
        public INetworkBroadcaster UDPBroadcaster { get; }

        private BinaryFormatter _formatter;
        private Dictionary<Guid, SortedList<long, Segment>> _segmentBuffer;

        public BroadcastChainer(INetworkBroadcaster udpBroadcaster, int segmentPayloadLength = 8192)
        {
            UDPBroadcaster = udpBroadcaster;
            SegmentPayloadLength = segmentPayloadLength;
            _formatter = new BinaryFormatter();
            _segmentBuffer = new Dictionary<Guid, SortedList<long, Segment>>();
        }

        public Task<long> SendAsync(byte[] payload)
        {
            return Task.Run(async () =>
            {
                var sequenceGUID = Guid.NewGuid();
                var sequenceLength = payload.LongLength % SegmentPayloadLength > 0 ?
                                     payload.LongLength / SegmentPayloadLength + 1 :
                                     payload.LongLength / SegmentPayloadLength;

                for (int i = 0; i < sequenceLength; i++)
                {
                    var beginWith = i * SegmentPayloadLength;

                    int endBefore = i + 1 == sequenceLength ?
                                             payload.Length :
                                             beginWith + SegmentPayloadLength;

                    var segment = new Segment(i, payload[beginWith..endBefore]);
                    var packet = new Packet(sequenceGUID, sequenceLength, segment);

                    byte[] bytes = _formatter.OneLineSerialize(packet);
                    await UDPBroadcaster.SendAsync(bytes);
                }

                return payload.LongLength;
            });
        }

        public async Task<byte[]> ReceiveAsync()
        {
            while (true)
            {
                var bytes = await UDPBroadcaster.ReceiveAsync();

                Packet packet = _formatter.OneLineDeserialize<Packet>(bytes);

                if (_segmentBuffer.ContainsKey(packet.SequenceGUID))
                {
                    _segmentBuffer[packet.SequenceGUID].Add(packet.Segment.SequenceIndex, packet.Segment);
                    if (_segmentBuffer[packet.SequenceGUID].Last().Key + 1 == packet.SequenceLength)
                    {
                        var messageLength = _segmentBuffer[packet.SequenceGUID].Values.Sum(segment => segment.Payload.LongLength);
                        var message = new byte[messageLength];
                        var messageOffset = 0;
                        foreach (var segment in _segmentBuffer[packet.SequenceGUID].Values)
                        {
                            Buffer.BlockCopy(segment.Payload, 0, message, messageOffset, segment.Payload.Length);
                            messageOffset += segment.Payload.Length;
                        }
                        _segmentBuffer.Remove(packet.SequenceGUID);
                        return message;
                    }
                }
                else
                {
                    if (packet.SequenceLength == 1)
                    {
                        return packet.Segment.Payload;
                    }

                    var segments = new SortedList<long, Segment>();
                    segments.Add(packet.Segment.SequenceIndex, packet.Segment);
                    _segmentBuffer.Add(packet.SequenceGUID, segments);
                }
            }
        }

        public ValueTask ClearBufferAsync()
        {
            _segmentBuffer.Clear();
            return UDPBroadcaster.ClearBufferAsync();
        }

        public void Dispose() => UDPBroadcaster?.Dispose();
    }
}
