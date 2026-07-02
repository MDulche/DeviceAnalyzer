using System.Management;

namespace DeviceAnalyzer.Models;

public enum HighlightState { None, New, Removed }

public enum DeviceCategory
{
    All, USB, HID, Audio, Disk, Network, Bluetooth, Other
}

public class PnpDevice
{
    public string Name { get; }
    public string Class { get; }
    public string Status { get; }
    public string Manufacturer { get; }
    public string PnpDeviceId { get; }
    public string InstanceId { get; }
    public string Description { get; }
    public string DriverVersion { get; }
    public string DriverDate { get; }
    public string HardwareId { get; }
    public bool IsPresent { get; }
    public HighlightState HighlightState { get; set; } = HighlightState.None;
    public DeviceCategory Category { get; }

    public bool IsError => Status is not "" and not "OK";
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "(Périphérique inconnu)" : Name;

    public string StatusDotColor => Status switch
    {
        "OK" => "#2E7D32",
        "Error" => "#C62828",
        "Degraded" => "#E65100",
        "Pred Fail" => "#E65100",
        _ => "#BDBDBD"
    };

    public string CategoryBadgeColor => Category switch
    {
        DeviceCategory.USB => "#1565C0",
        DeviceCategory.HID => "#6A1B9A",
        DeviceCategory.Audio => "#E65100",
        DeviceCategory.Disk => "#2E7D32",
        DeviceCategory.Network => "#00838F",
        DeviceCategory.Bluetooth => "#1565C0",
        _ => "#757575"
    };

    public string CategoryBadgeText => Category switch
    {
        DeviceCategory.USB => "USB",
        DeviceCategory.HID => "HID",
        DeviceCategory.Audio => "AUDIO",
        DeviceCategory.Disk => "DISQUE",
        DeviceCategory.Network => "RÉSEAU",
        DeviceCategory.Bluetooth => "BT",
        _ => "AUTRE"
    };

    public PnpDevice(ManagementBaseObject mo)
    {
        Name = GetString(mo, "Name");
        Class = GetString(mo, "PNPClass");
        Status = GetString(mo, "Status");
        Manufacturer = GetString(mo, "Manufacturer");
        PnpDeviceId = GetString(mo, "PNPDeviceID");
        InstanceId = GetString(mo, "DeviceID");
        Description = GetString(mo, "Description");
        DriverVersion = GetString(mo, "DriverVersion");
        DriverDate = GetString(mo, "DriverDate");
        HardwareId = string.Join(", ", GetStringArray(mo, "HardwareID"));
        IsPresent = string.Equals(GetString(mo, "Present"), "True", StringComparison.OrdinalIgnoreCase);
        Category = DetectCategory();
    }

    private DeviceCategory DetectCategory()
    {
        var cls = Class.ToUpperInvariant();
        var pnp = PnpDeviceId.ToUpperInvariant();
        var name = Name.ToUpperInvariant();

        if (cls is "BLUETOOTH" or "BLUETOOTHDEVICE" || pnp.Contains("BTH") || pnp.Contains("BLUETOOTH"))
            return DeviceCategory.Bluetooth;

        if (cls is "NET" or "NETWORKADAPTER" or "NIC" ||
            name.Contains("ETHERNET") || name.Contains("WIFI") || name.Contains("WIRELESS") ||
            name.Contains("LAN") || name.Contains("WLAN") || name.Contains("802.11"))
            return DeviceCategory.Network;

        if (cls is "MEDIA" or "AUDIOENDPOINT" or "SOUND" or "SOUNDDEVICE" or "AUDIO" or "AUDIOADAPTER" or "DRIVER" or "WDMAUD")
            return DeviceCategory.Audio;

        if (cls is "HIDCLASS" or "KEYBOARD" or "MOUSE" or "POINTER" or "HID")
            return DeviceCategory.HID;

        if (cls is "DISKDRIVE" or "VOLUME" or "CDROM" or "STORAGE" or "DISK" or "SCSIADAPTER")
            return DeviceCategory.Disk;

        if (cls is "USB" or "USBDEVICE" || pnp.Contains("USB"))
            return DeviceCategory.USB;

        return DeviceCategory.Other;
    }

    private static string GetString(ManagementBaseObject mo, string property)
    {
        try { return mo[property]?.ToString()?.Trim() ?? ""; }
        catch { return ""; }
    }

    private static string[] GetStringArray(ManagementBaseObject mo, string property)
    {
        try { if (mo[property] is string[] arr) return arr; }
        catch { }
        return [];
    }
}
