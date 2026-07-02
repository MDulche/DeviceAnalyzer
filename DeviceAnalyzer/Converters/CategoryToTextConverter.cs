using System.Globalization;
using System.Windows.Data;
using DeviceAnalyzer.Models;

namespace DeviceAnalyzer.Converters;

public class CategoryToTextConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is DeviceCategory cat
            ? cat switch
            {
                DeviceCategory.USB => "USB",
                DeviceCategory.HID => "HID",
                DeviceCategory.Audio => "AUDIO",
                DeviceCategory.Disk => "DISQUE",
                DeviceCategory.Network => "RÉSEAU",
                DeviceCategory.Bluetooth => "BT",
                _ => "AUTRE"
            }
            : "AUTRE";
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
