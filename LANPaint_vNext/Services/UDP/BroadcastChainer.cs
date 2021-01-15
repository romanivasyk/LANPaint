using LANPaint_vNext.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP
{
    public class BroadcastChainer : IDisposable
    {
        public int PayloadSegmentLength { get; } = 8192;

        private INetworkBroadcaster _udpBroadcaster;
        private BinaryFormatter _formatter = new BinaryFormatter();
#warning TODO: Add Dictionary cleanup by timeout(We don't have to waste memory for data sequences that never be assembled due to random packet loss)
        private Dictionary<Guid, SortedList<long, Segment>> _receivePacketBuffer = new Dictionary<Guid, SortedList<long, Segment>>();

        public BroadcastChainer() : this(new UDPBroadcastImpl())
        { }

        public BroadcastChainer(INetworkBroadcaster udpBroadcaster)
        {
            _udpBroadcaster = udpBroadcaster;
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

                    byte[] bytes = _formatter.OneLineSerialize(packet);
                    await _udpBroadcaster.SendAsync(bytes);
                }

                return payload.LongLength;
            });
        }

        public async Task<byte[]> ReceiveAsync()
        {
            while (true)
            {
                var bytes = await _udpBroadcaster.ReceiveAsync();

                Packet packet = _formatter.OneLineDeserialize<Packet>(bytes);

                if (_receivePacketBuffer.ContainsKey(packet.SequenceGUID))
                {
                    _receivePacketBuffer[packet.SequenceGUID].Add(packet.Segment.SequenceIndex, packet.Segment);
                    if (_receivePacketBuffer[packet.SequenceGUID].Last().Key + 1 == packet.SequenceLength)
                    {
                        var messageLength = _receivePacketBuffer[packet.SequenceGUID].Values.Sum(segment => segment.Payload.LongLength);
                        var message = new byte[messageLength];
                        var messageOffset = 0;
                        foreach (var segment in _receivePacketBuffer[packet.SequenceGUID].Values)
                        {
                            Buffer.BlockCopy(segment.Payload, 0, message, messageOffset, segment.Payload.Length);
                            messageOffset += segment.Payload.Length;
                        }
                        _receivePacketBuffer.Remove(packet.SequenceGUID);
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
                    _receivePacketBuffer.Add(packet.SequenceGUID, segments);
                }
            }
        }

        public void Dispose()
        {
            _udpBroadcaster?.Dispose();
        }
    }
}
