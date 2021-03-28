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
        [InlineData(null, "")]
        [Theory]
        public void ConvertValidNullableInt(int? intToConvert, string expected)
        {
            var converter = new NullableIntToStringConverter();
            var result = (string) converter.Convert(intToConvert, typeof(string), null, CultureInfo.CurrentCulture);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertNotSupportedType()
        {
            double? doubleToConvert = 1.5;
            var converter = new NullableIntToStringConverter();
            Assert.Throws<InvalidOperationException>(() =>
                (string) converter.Convert(doubleToConvert, typeof(string), null, CultureInfo.CurrentCulture));
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

        [Fact]
        public void ConvertBackNotSupportedType()
        {
            const int intToConvert = 1;
            var converter = new NullableIntToStringConverter();
            Assert.Throws<InvalidOperationException>(() =>
                (int?) converter.ConvertBack(intToConvert, typeof(int?), null, CultureInfo.CurrentCulture));
        }
    }
}