using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

namespace LANPaint_vNext.Services.UDP.Test
{
    public class BroadcastChainer : IDisposable
    {
        public int ChainLength { get; } = 8192;
        private UDPBroadcastBase _udpBroadcaster;
        private IFormatter _formatter;

        private Dictionary<Guid, SortedList<long, Segment>> _receivePacketBuffer = new Dictionary<Guid, SortedList<long, Segment>>();
        public BroadcastChainer()
        {
            _udpBroadcaster = new UDPBroadcastImpl();
            _formatter = new BinaryFormatter();
        }

        public BroadcastChainer(UDPBroadcastBase udpBroadcaster) : this(udpBroadcaster, new BinaryFormatter())
        { }

        public BroadcastChainer(UDPBroadcastBase udpBroadcaster, IFormatter formatter)
        {
            _udpBroadcaster = udpBroadcaster;
            _formatter = formatter;
        }

        public Task<long> SendAsync(byte[] payload)
        {
            return Task.Run(async () =>
            {
                var sequenceGUID = Guid.NewGuid();
                var sequenceLength = payload.LongLength % ChainLength > 0 ?
                                     payload.LongLength / ChainLength + 1 :
                                     payload.LongLength / ChainLength;

                for (int i = 0; i < sequenceLength; i++)
                {
                    var beginWith = i * ChainLength;
                    int endBefore;

                    if (i + 1 == sequenceLength)
                    {
                        endBefore = payload.Length;
                    }
                    else
                    {
                        endBefore = beginWith + ChainLength;
                    }

                    var segment = new Segment(i, payload[beginWith..endBefore]);
                    var packet = new Packet(sequenceGUID, sequenceLength, segment);

                    byte[] bytes = null;
                    using (var stream = new MemoryStream())
                    {
                        _formatter.Serialize(stream, packet);
                        bytes = stream.ToArray();
                    }

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

                if (bytes == null)
                {
                    continue;
                }

                Packet packet;
                using (var stream = new MemoryStream(bytes))
                {
                    packet = (Packet)_formatter.Deserialize(stream);
                }

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
