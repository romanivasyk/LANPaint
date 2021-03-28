using System;
using System.Globalization;
using LANPaint.Converters;
using Xunit;

namespace LANPaint.UnitTests.Converters
{
    public class StringToDoubleTests
    {
        [InlineData("5", 5)]
        [InlineData("3.4", 3.4)]
        [InlineData("-3.4", -3.4)]
        [InlineData("0", 0)]
        [InlineData("", 0)]
        [InlineData(null, 0)]
        [Theory]
        public void ConvertTheory(string stringToConvert, double expected)
        {
            var converter = new StringToDoubleConverter();
            var result = (double) converter.Convert(stringToConvert, typeof(double), null, CultureInfo.CurrentCulture);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertNotSupportedType()
        {
            const int intToConvert = 1;
            var converter = new StringToDoubleConverter();
            Assert.Throws<InvalidOperationException>(() => converter.Convert(intToConvert, typeof(double), null,
                CultureInfo.CurrentCulture));
        }

        [InlineData(5, "5")]
        [InlineData(3.4, "3.4")]
        [InlineData(-3.4, "-3.4")]
        [InlineData(0, "0")]
        [Theory]
        public void ConvertBackTheory(double doubleToConvert, string expected)
        {
            var converter = new StringToDoubleConverter();
            var result = (string) converter.ConvertBack(doubleToConvert, typeof(string), null, CultureInfo.CurrentCulture);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ConvertBackNotSupportedType()
        {
            const int intToConvert = 1;
            var converter = new StringToDoubleConverter();
            Assert.Throws<InvalidOperationException>(() => converter.ConvertBack(intToConvert, typeof(string), null,
                CultureInfo.CurrentCulture));
        }
    }
}