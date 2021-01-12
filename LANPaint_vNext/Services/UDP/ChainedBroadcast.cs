using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Runtime.Serialization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Buffers;

namespace LANPaint_vNext.Services.UDP
{
    public class ChainedBroadcast : IDisposable
    {
        public int ChainLength { get; } = 8192;
        private UDPBroadcastBase _udpWrapper;
        private IFormatter _formatter;

        private Dictionary<Guid, SortedList<long, Segment>> _receivedPackets = new Dictionary<Guid, SortedList<long, Segment>>();
        public ChainedBroadcast()
        {
            _udpWrapper = new UDPBroadcastImpl();
            _formatter = new BinaryFormatter();
        }

        public Task<long> SendAsync(byte[] payload)
        {
            return Task.Run(() =>
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

                    _udpWrapper.SendAsync(bytes).GetAwaiter().GetResult();
                }

                return payload.LongLength;
            });
        }
        public async Task<byte[]> ReceiveAsync()
        {
            while (true)
            {
                var bytes = await _udpWrapper.ReceiveAsync();

                if(bytes == null)
                {
                    continue;
                }

                Packet packet;
                using (var stream = new MemoryStream(bytes))
                {
                    packet = (Packet)_formatter.Deserialize(stream);
                }

                if (_receivedPackets.ContainsKey(packet.SequenceGUID))
                {
                    _receivedPackets[packet.SequenceGUID].Add(packet.Segment.Index, packet.Segment);
                    if (_receivedPackets[packet.SequenceGUID].Last().Key + 1 == packet.SequenceLength)
                    {
                        var messageLength = _receivedPackets[packet.SequenceGUID].Values.Sum(segment => segment.Payload.LongLength);
                        var message = new byte[messageLength];
                        var messageOffset = 0;
                        foreach (var segment in _receivedPackets[packet.SequenceGUID].Values)
                        {
                            Buffer.BlockCopy(segment.Payload, 0, message, messageOffset, segment.Payload.Length);
                            messageOffset += segment.Payload.Length;
                        }
                        _receivedPackets.Remove(packet.SequenceGUID);
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
                    segments.Add(packet.Segment.Index, packet.Segment);
                    _receivedPackets.Add(packet.SequenceGUID, segments);
                }
            }
        }

        public void Dispose()
        {
            _udpWrapper?.Dispose();
        }
    }
}
