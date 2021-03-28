using System;
using System.Globalization;
using System.Windows.Data;

namespace LANPaint.Converters
{
    [ValueConversion(typeof(int?), typeof(string))]
    public class NullableIntToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int && value is not null) throw new ArgumentException("Value type is not Nullable<int>");
            return value?.ToString() ?? string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value?.ToString()) ? null : new int?(int.Parse(value.ToString()));
        }
    }
}