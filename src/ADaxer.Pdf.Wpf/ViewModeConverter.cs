using System.Globalization;
using System.Windows.Data;

namespace ADaxer.Pdf.Wpf;
public class ViewModeConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return ((PdfViewModes)value) == PdfViewModes.Double ? System.Windows.Controls.Orientation.Horizontal : System.Windows.Controls.Orientation.Vertical;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
