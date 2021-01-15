using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LANPaint.Converters
{
    [ValueConversion(typeof(Color), typeof(Brush))]
    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }

            return new SolidColorBrush((Color)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }

            return ((SolidColorBrush)value).Color;
        }
    }
}
