using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using LANPaint.Extensions;
using Xunit;

namespace LANPaint.UnitTests.Extensions
{
    public class BinaryFormatterExtensionTests
    {
        [Fact]
        public void SerializeDeserialize()
        {
            var point = new Point(10, 152);
            var formatter = new BinaryFormatter();

            var bytes = formatter.OneLineSerialize(point);
            var deserializedPoint = (Point) formatter.OneLineDeserialize(bytes);

            Assert.Equal(point, deserializedPoint);
        }
        
        [Fact]
        public void SerializePassNull()
        {
            var formatter = new BinaryFormatter();
            Assert.Throws<ArgumentNullException>(()=>formatter.OneLineSerialize(null));
        }
        
        [Fact]
        public void DeserializePassEmptyCollection()
        {
            var bytes = Array.Empty<byte>();
            var formatter = new BinaryFormatter();
            Assert.Throws<ArgumentException>(()=>formatter.OneLineDeserialize(bytes));
        }
        
        [Fact]
        public void DeserializePassNull()
        {
            var formatter = new BinaryFormatter();
            Assert.Throws<ArgumentNullException>(()=>formatter.OneLineDeserialize(null));
        }
    }
}