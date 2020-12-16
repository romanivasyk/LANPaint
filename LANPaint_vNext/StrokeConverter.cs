using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Ink;

namespace LANPaint_vNext
{
    [ValueConversion(typeof(ObservableCollection<Stroke>), typeof(StrokeCollection))]
    class StrokeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new StrokeCollection((ObservableCollection<Stroke>)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return new ObservableCollection<Stroke>((StrokeCollection)value);
        }
    }
}
