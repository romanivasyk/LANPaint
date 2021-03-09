using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LANPaint.Converters
{
    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class ToolbarOverflowVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return (bool) value ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            return (Visibility) value switch
            {
                Visibility.Visible => true,
                Visibility.Hidden => false,
                Visibility.Collapsed => false
            };
        }
    }
}