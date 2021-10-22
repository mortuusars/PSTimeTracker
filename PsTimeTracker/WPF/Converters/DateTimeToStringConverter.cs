using System;
using System.Globalization;
using System.Windows.Data;

namespace PSTimeTracker.WPF.Converters
{
    [ValueConversion(typeof(DateTimeOffset), typeof(string))]
    public class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTimeOffset time = (DateTimeOffset)value;

            return time.ToString("MMM-dd HH:mm");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
