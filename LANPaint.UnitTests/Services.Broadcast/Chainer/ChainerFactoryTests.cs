using System;
using System.Net;
using LANPaint.Services.Broadcast.Factories;
using Xunit;

namespace LANPaint.UnitTests.Services.Broadcast.Chainer
{
    public class ChainerFactoryTests
    {
        [Fact]
        public void InvalidCtorParam_LessThanMinSegmentLength()
        {
            var segmentLength = 0;
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChainerFactory(segmentLength));
        }
        
        [Fact]
        public void InvalidCtorParam_MoreThanMaxSegmentLength()
        {
            var segmentLength = 100000;
            Assert.Throws<ArgumentOutOfRangeException>(() => new ChainerFactory(segmentLength));
        }
        
        [Fact]
        public void ValidCtorParam()
        {
            var segmentLength = 16384;
            var factory = new ChainerFactory(segmentLength);
            Assert.Equal(segmentLength, factory.PayloadSegmentLength);
        }

        [Fact]
        public void Create_NullAddressParam()
        {
            var segmentLength = 16384;
            IPAddress address = default;
            var factory = new ChainerFactory(segmentLength);

            Assert.Throws<ArgumentNullException>(() => factory.Create(address));
        }
        
        // [Fact]
        // public void Create()
        // {
        //     var segmentLength = 16384;
        //     
        //     //192.168.0.50
        //     //IPAddress address = new IPAddress(3232235570);
        //     //192.168.0.100
        //     var address = IPAddress.Parse("192.168.0.100");
        //     var factory = new ChainerFactory(segmentLength);
        //
        //     using var chainer = factory.Create(address);
        //     Assert.Equal(address, chainer.LocalEndPoint.Address);
        // }
        
        [Fact]
        public void Create_NegativePortValue()
        {
            var segmentLength = 16384;
            var address = IPAddress.Parse("192.168.0.50");
            var port = -1;
            var factory = new ChainerFactory(segmentLength);

            Assert.Throws<ArgumentOutOfRangeException>(() => factory.Create(address, port));
        }
    }
}