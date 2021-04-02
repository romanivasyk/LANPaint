using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using LANPaint.Extensions;

namespace LANPaint.Services.Broadcast.Decorators
{
    public class Chainer : BroadcastDecorator
    {
        public const int MinSegmentLength = 1024;
        public const int MaxSegmentLength = 65535;

        private readonly int _payloadSegmentLength;
        private readonly BinaryFormatter _formatter;
        private readonly Dictionary<Guid, SortedList<long, Segment>> _segmentBuffer;

        public Chainer(IBroadcast broadcaster, int payloadSegmentLength = 8192) : base(broadcaster)
        {
            if (payloadSegmentLength < MinSegmentLength || payloadSegmentLength > MaxSegmentLength)
                throw new ArgumentOutOfRangeException(nameof(payloadSegmentLength),
                    $"Provided segment length should be in range from {Chainer.MinSegmentLength} to {Chainer.MaxSegmentLength}");

            _payloadSegmentLength = payloadSegmentLength;
            _formatter = new BinaryFormatter();
            _segmentBuffer = new Dictionary<Guid, SortedList<long, Segment>>();
        }

        public override async Task<int> SendAsync(byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            var sequenceGuid = Guid.NewGuid();
            var sequenceLength = payload.Length % _payloadSegmentLength > 0
                ? payload.Length / _payloadSegmentLength + 1
                : payload.Length / _payloadSegmentLength;

            var bytesSentCount = 0;
            for (var i = 0; i < sequenceLength; i++)
            {
                var beginWith = i * _payloadSegmentLength;
                var endBefore = i + 1 == sequenceLength ? payload.Length : beginWith + _payloadSegmentLength;

                var segment = new Segment(i, payload[beginWith..endBefore]);
                var packet = new Packet(sequenceGuid, sequenceLength, segment);

                var bytes = _formatter.OneLineSerialize(packet);
                bytesSentCount += await Broadcast.SendAsync(bytes);
            }

            return bytesSentCount;
        }

        public override async Task<byte[]> ReceiveAsync(CancellationToken token = default)
        {
            while (true)
            {
                byte[] bytes;
                try
                {
                    token.ThrowIfCancellationRequested();
                    bytes = await Broadcast.ReceiveAsync(token);
                }
                catch
                {
                    _segmentBuffer.Clear();
                    throw;
                }

                Packet packet;
                try
                {
                    packet = (Packet) _formatter.OneLineDeserialize(bytes);
                }
                catch (SerializationException)
                {
                    continue;
                }


                if (_segmentBuffer.ContainsKey(packet.SequenceGuid))
                {
                    _segmentBuffer[packet.SequenceGuid].Add(packet.Segment.SequenceIndex, packet.Segment);

                    if (_segmentBuffer[packet.SequenceGuid].Count != packet.SequenceLength ||
                        _segmentBuffer[packet.SequenceGuid].Last().Key + 1 != packet.SequenceLength) continue;

                    var messageLength = _segmentBuffer[packet.SequenceGuid].Values
                        .Sum(segment => segment.Payload.LongLength);
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

                if (packet.SequenceLength == 1)
                {
                    return packet.Segment.Payload;
                }

                var segments = new SortedList<long, Segment>
                {
                    {packet.Segment.SequenceIndex, packet.Segment}
                };
                _segmentBuffer.Add(packet.SequenceGuid, segments);
            }
        }

        public override ValueTask ClearBufferAsync() => Broadcast.ClearBufferAsync();

        public override void Dispose() => Broadcast?.Dispose();
    }
}