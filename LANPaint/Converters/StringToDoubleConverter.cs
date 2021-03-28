using System;
using System.Globalization;
using System.Windows.Data;

namespace LANPaint.Converters
{
    [ValueConversion(typeof(string), typeof(double))]
    public class StringToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string || value is null)
                return string.IsNullOrEmpty(value?.ToString()) ? 0 : double.Parse((string) value);
            throw new InvalidOperationException($"The type of {nameof(value)} is not string");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double || value is null)
                return string.IsNullOrEmpty(value?.ToString()) ? string.Empty : value.ToString();
            throw new InvalidOperationException($"The type of {nameof(value)} is not double");
        }
    }
}