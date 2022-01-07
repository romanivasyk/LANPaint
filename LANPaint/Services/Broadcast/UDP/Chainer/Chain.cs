using System;
using System.Collections.Generic;
using System.Linq;

namespace LANPaint.Services.Broadcast.UDP.Chainer;

public class Chain
{
    public DateTime LastAddedAt { get; private set; }

    private readonly long _chainLength;
    private SortedSet<Segment> _sortedSegments;

    public Chain(long chainLength)
    {
        if (chainLength <= 0) throw new ArgumentOutOfRangeException(nameof(chainLength));
        _chainLength = chainLength;
    }

    public bool AddSegment(Segment segment)
    {
        _sortedSegments ??= new SortedSet<Segment>();

        var isAdded = _sortedSegments.Add(segment);
        if (isAdded) LastAddedAt = DateTime.Now;
        return isAdded;
    }

    public bool TryAssemble(out byte[] bytes)
    {
        if (_sortedSegments is null || _sortedSegments.Count != _chainLength)
        {
            bytes = null;
            return false;
        }

        var bytesCount = _sortedSegments.Sum(segment => segment.Payload.Length);
        var assembledBytes = new byte[bytesCount];

        var nextFreeIndex = 0;
        foreach (var segment in _sortedSegments)
        {
            segment.Payload.CopyTo(assembledBytes, nextFreeIndex);
            nextFreeIndex += segment.Payload.Length;
        }

        bytes = assembledBytes;
        return true;
    }
}