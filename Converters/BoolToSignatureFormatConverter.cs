using BookbindingPdfMaker.Models;
using System.Globalization;
using System.Windows.Data;

namespace BookbindingPdfMaker.Converters
{
    public class BoolToSignatureFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Enum.TryParse(parameter.ToString(), out SignatureFormat printerType);
            return (SignatureFormat)value == printerType;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter;
        }
    }
}
