using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using DeviceAnalyzer.Models;

namespace DeviceAnalyzer.Converters;

public class CategoryToColorConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var brush = value is DeviceCategory cat
            ? cat switch
            {
                DeviceCategory.USB => new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)),
                DeviceCategory.HID => new SolidColorBrush(Color.FromRgb(0x6A, 0x1B, 0x9A)),
                DeviceCategory.Audio => new SolidColorBrush(Color.FromRgb(0xE6, 0x51, 0x00)),
                DeviceCategory.Disk => new SolidColorBrush(Color.FromRgb(0x2E, 0x7D, 0x32)),
                DeviceCategory.Network => new SolidColorBrush(Color.FromRgb(0x00, 0x83, 0x8F)),
                DeviceCategory.Bluetooth => new SolidColorBrush(Color.FromRgb(0x15, 0x65, 0xC0)),
                _ => new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75))
            }
            : new SolidColorBrush(Color.FromRgb(0x75, 0x75, 0x75));

        brush.Freeze();
        return brush;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
