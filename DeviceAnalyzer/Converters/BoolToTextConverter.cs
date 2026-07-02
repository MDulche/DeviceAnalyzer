using System.Globalization;
using System.Windows.Data;

namespace DeviceAnalyzer.Converters;

public class BoolToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isPresent = value is true;
        return isPresent ? "Oui" : "Non";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
