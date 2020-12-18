using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace LANPaint_vNext
{
    [ValueConversion(typeof(Brush), typeof(Nullable<Color>))]
    public class BrushColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is null)
            {
                return null;
            }

            return ((SolidColorBrush)value).Color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is null)
            {
                return null;
            }

            return new SolidColorBrush((Color)value);
        }
    }
}
