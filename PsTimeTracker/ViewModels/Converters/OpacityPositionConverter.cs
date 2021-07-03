using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PSTimeTracker
{
    public class OpacityPositionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double width = (double)value;
            if ((string)parameter == "transparent")
            {
                return new Point(width - 74, 0);
            }
            
            return new Point(width - 125, 0);
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
