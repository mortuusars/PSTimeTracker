using System;
using System.Globalization;
using System.Windows.Data;

namespace PSTimeTracker.WPF.Converters
{
    [ValueConversion(typeof(int), typeof(string))]
    public class SecondsToTimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.GetType() == typeof(int))
                return TimeFormatter.GetTimeStringFromSeconds((int)value);
            else 
                return TimeFormatter.GetTimeStringFromSeconds((long)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (int)((TimeSpan)value).TotalSeconds;
        }
    }
}
