using System;
using System.Globalization;
using System.Windows.Data;

namespace LANPaint.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class StringToIntConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value?.ToString()) ? 0 : int.Parse(value.ToString());
        }
    }
}
