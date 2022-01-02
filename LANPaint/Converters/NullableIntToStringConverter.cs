using System;
using System.Globalization;
using System.Windows.Data;

namespace LANPaint.Converters;

[ValueConversion(typeof(int?), typeof(string))]
public class NullableIntToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is int || value is null) return value?.ToString() ?? string.Empty;
        throw new InvalidOperationException($"The type of {nameof(value)} is not Nullable<int>");
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string || value is null)
            return string.IsNullOrEmpty(value?.ToString()) ? null : new int?(int.Parse(value.ToString()));
        throw new InvalidOperationException($"The type of {nameof(value)} is not string");
    }
}