using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace DeviceAnalyzer.Helpers;

public class NullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var invert = parameter is string s && s == "invert";
        var isNull = value is null;

        return (isNull ^ invert) ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
