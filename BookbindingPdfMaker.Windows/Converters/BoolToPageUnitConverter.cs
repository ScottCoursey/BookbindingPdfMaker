using BookbindingPdfMaker.Models;
using System.Globalization;
using System.Windows.Data;

namespace BookbindingPdfMaker.Converters
{
    public class BoolToPageUnitConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(parameter.ToString(), out PageUnit temp);
            return (PageUnit)value == temp;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
