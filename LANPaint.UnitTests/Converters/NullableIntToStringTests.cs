using System;
using System.Globalization;
using LANPaint.Converters;
using Xunit;

namespace LANPaint.UnitTests.Converters
{
    public class NullableIntToStringTests
    {
        [InlineData(5, "5")]
        [InlineData(-5, "-5")]
        [InlineData(0, "0")]
        [Theory]
        public void ConvertValidInt(int? intToConvert, string expected)
        {
            var converter = new NullableIntToStringConverter();
            var result = (string) converter.Convert(intToConvert, typeof(string), null, CultureInfo.CurrentCulture);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertNullInt()
        {
            int? nullableInt = default;
            var converter = new NullableIntToStringConverter();
            var result = (string) converter.Convert(nullableInt, typeof(string), null, CultureInfo.CurrentCulture);

            Assert.Equal(string.Empty, result);
        }

        [Fact]
        public void ConvertNotSupportedType()
        {
            double? doubleToConvert = 1.5;
            var converter = new NullableIntToStringConverter();
            Assert.Throws<ArgumentException>(() =>
                (string) converter.Convert(doubleToConvert, typeof(string), null, CultureInfo.CurrentCulture));
        }

        [Fact]
        public void ConvertNullNotSupportedType()
        {
            long? nullableLong = null;
            var converter = new NullableIntToStringConverter();
            var result = (string) converter.Convert(nullableLong, typeof(string), null, CultureInfo.CurrentCulture);

            Assert.Equal(string.Empty, result);
        }

        [InlineData("0", 0)]
        [InlineData("5", 5)]
        [InlineData("-5", -5)]
        [InlineData("", null)]
        [InlineData(null, null)]
        [Theory]
        public void ConvertBackValidString(string stringToConvert, int? expected)
        {
            var converter = new NullableIntToStringConverter();
            var result = (int?) converter.ConvertBack(stringToConvert, typeof(int?), null, CultureInfo.CurrentCulture);

            Assert.Equal(expected, result);
        }

        [InlineData("asd")]
        [InlineData("3.4")]
        [InlineData("--5")]
        [InlineData("54f")]
        [Theory]
        public void ConvertBackInvalidString(string invalidStringToConvert)
        {
            var converter = new NullableIntToStringConverter();
            Assert.Throws<FormatException>(() =>
                (int?) converter.ConvertBack(invalidStringToConvert, typeof(int?), null, CultureInfo.CurrentCulture));
        }
    }
}