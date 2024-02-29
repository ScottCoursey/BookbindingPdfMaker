using BookbindingPdfMaker.Models;
using System.Globalization;
using System.Windows.Data;

namespace BookbindingPdfMaker.Converters
{
    public class BoolToStringHasContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = value?.ToString();
            var r = !string.IsNullOrEmpty(val) && val != Constants.NoFileSelected;
            return r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
