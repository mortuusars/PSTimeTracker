using System;
using System.Globalization;
using System.Windows.Data;

namespace PSTimeTracker
{
    [ValueConversion(typeof(int), typeof(string))]
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return TimeFormatter.GetTimeStringFromSecods((int)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((TimeSpan)value).TotalSeconds;
        }
    }
}
