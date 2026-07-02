using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DeviceAnalyzer.Converters;

public class StatusToForegroundConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var status = value as string;
        var brush = status switch
        {
            "OK" => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
            "Error" => new SolidColorBrush(Color.FromRgb(0xC6, 0x28, 0x28)),
            "Degraded" or "Pred Fail" => new SolidColorBrush(Color.FromRgb(0xE6, 0x51, 0x00)),
            _ => new SolidColorBrush(Color.FromRgb(0x55, 0x55, 0x55))
        };
        brush.Freeze();
        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
