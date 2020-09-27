using System;
using System.Globalization;
using System.Windows.Data;

namespace PhotoshopTimeCounter
{
    [ValueConversion(typeof(int), typeof(string))]
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            
            return TimeSpan.FromSeconds((int)value).ToString();

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            return (int)((TimeSpan)value).TotalSeconds;
        }
    }
}
