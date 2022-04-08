using System;
using System.Windows.Data;

namespace Konbini.RfidFridge.TagManagement.Common
{
    public class IntToCashConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            int.TryParse(value.ToString(), out var valueOut);
            return valueOut >= 0 ? $"${valueOut / 100.0:0.00}" : $"(${-valueOut / 100.0:0.00})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
            // return (int)(Double.Parse(((string)value).Substring(1)) * 100);
        }
    }
}
