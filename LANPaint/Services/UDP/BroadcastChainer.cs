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
        public int PayloadSegmentLength { get; } = 8192;

        public INetworkBroadcaster UDPBroadcaster { get; }
        public BinaryFormatter Formatter { get; } = new BinaryFormatter();
#warning TODO: Add Dictionary cleanup by timeout(We don't have to waste memory for data sequences that never be assembled due to random packet loss)
        public Dictionary<Guid, SortedList<long, Segment>> SegmentBuffer { get; } = new Dictionary<Guid, SortedList<long, Segment>>();

        public BroadcastChainer() : this(new UDPBroadcastImpl())
        { }

        public BroadcastChainer(INetworkBroadcaster udpBroadcaster)
        {
            UDPBroadcaster = udpBroadcaster;
        }

        public Task<long> SendAsync(byte[] payload)
        {
            return Task.Run(async () =>
            {
                var sequenceGUID = Guid.NewGuid();
                var sequenceLength = payload.LongLength % PayloadSegmentLength > 0 ?
                                     payload.LongLength / PayloadSegmentLength + 1 :
                                     payload.LongLength / PayloadSegmentLength;

                for (int i = 0; i < sequenceLength; i++)
                {
                    var beginWith = i * PayloadSegmentLength;

                    int endBefore = i + 1 == sequenceLength ?
                                             payload.Length :
                                             beginWith + PayloadSegmentLength;

                    var segment = new Segment(i, payload[beginWith..endBefore]);
                    var packet = new Packet(sequenceGUID, sequenceLength, segment);

                    byte[] bytes = Formatter.OneLineSerialize(packet);
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

                Packet packet = Formatter.OneLineDeserialize<Packet>(bytes);

                if (SegmentBuffer.ContainsKey(packet.SequenceGUID))
                {
                    SegmentBuffer[packet.SequenceGUID].Add(packet.Segment.SequenceIndex, packet.Segment);
                    if (SegmentBuffer[packet.SequenceGUID].Last().Key + 1 == packet.SequenceLength)
                    {
                        var messageLength = SegmentBuffer[packet.SequenceGUID].Values.Sum(segment => segment.Payload.LongLength);
                        var message = new byte[messageLength];
                        var messageOffset = 0;
                        foreach (var segment in SegmentBuffer[packet.SequenceGUID].Values)
                        {
                            Buffer.BlockCopy(segment.Payload, 0, message, messageOffset, segment.Payload.Length);
                            messageOffset += segment.Payload.Length;
                        }
                        SegmentBuffer.Remove(packet.SequenceGUID);
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
                    SegmentBuffer.Add(packet.SequenceGUID, segments);
                }
            }
        }

        public ValueTask ClearBufferAsync() => UDPBroadcaster.ClearBufferAsync();

        public void Dispose() => UDPBroadcaster?.Dispose();
    }
}
