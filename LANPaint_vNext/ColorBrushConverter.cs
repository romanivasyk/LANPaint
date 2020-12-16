using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LANPaint_vNext
{
    [ValueConversion(typeof(Brush), typeof(Color))]
    public class ColorBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((SolidColorBrush)value).Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new SolidColorBrush((Color)value);
        }
    }
}
