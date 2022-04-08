using System;
using System.Windows.Data;

namespace Konbini.RfidFridge.TagManagement.Common
{
    public class BoolToRedOrBlackConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value) ? "Red" : "Black";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new Exception("not implemented");
        }
    }
}
