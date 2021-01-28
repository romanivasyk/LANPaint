using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LANPaint.Converters
{
    [ValueConversion(typeof(Brush), typeof(Nullable<Color>))]
    public class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SolidColorBrush) value)?.Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is null ? null : new SolidColorBrush((Color)value);
        }
    }
}
