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
    public int ConfigManagerErrorCode { get; }
    public DeviceCategory Category { get; }

    public HighlightState HighlightState
    {
        get => _highlightState;
        set
        {
            if (_highlightState != value)
            {
                _highlightState = value;
                PropertyChanged?.Invoke(this,
                    new System.ComponentModel.PropertyChangedEventArgs(nameof(HighlightState)));
            }
        }
    }
    private HighlightState _highlightState;

    public bool IsError => Status is not "" and not "OK" || ConfigManagerErrorCode > 0;
    public bool IsProblematic => IsError || !IsPresent || ConfigManagerErrorCode > 0;
    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? "(Périphérique inconnu)" : Name;

    public string ConfigManagerErrorDescription => ConfigManagerErrorCode switch
    {
        0  => "Aucune erreur",
        1  => "Pilote non configuré correctement",
        2  => "Pilote non chargé",
        3  => "Pilote endommagé ou manquant",
        4  => "Échec du pilote (code d’erreur protocolaire)",
        5  => "Échec de l’initialisation",
        6  => "Ressources insuffisantes",
        7  => "Absence de pilote",
        8  => "Fabricant du pilote inconnu",
        9  => "Mauvaise configuration du pilote",
        10 => "Conflit d’E/S",
        11 => "Périphérique retiré",
        12 => "Périphérique non disponible (en attente d’autres)",
        13 => "Stockage insuffisant",
        14 => "Matériel défaillant",
        15 => "Incompatibilité de ressources",
        16 => "Conflit de ressources",
        17 => "Pilote de filtre inconnu",
        18 => "Signature du pilote manquante",
        19 => "Périphérique marqué comme problème",
        20 => "Firmware corrompu",
        21 => "Pilote non certifié",
        22 => "Réinitialisation du périphérique",
        23 => "Conflit de ressources logicielles",
        24 => "Périphérique retiré (code obsolète)",
        25 => "Pilote non chargé (Démarrage désactivé)",
        26 => "Échec du pilote (démarrage)",
        27 => "Pilote incorrect",
        28 => "Pilote incompatible",
        29 => "Problème de ressources de carte fille",
        30 => "Périphérique incompatible avec la version de Windows",
        31 => "Périphérique en panne (défaillance matérielle)",
        32 => "Périphérique non activé (paramètre BIOS)",
        33 => "Périphérique non activé",
        34 => "Périphérique non disponible (firmware incompatible)",
        35 => "Périphérique non disponible (mode veille prolongée)",
        36 => "Périphérique non disponible (arrêt du système)",
        37 => "Problème de signature de pilote manquante (redémarrage requis)",
        38 => "Périphérique non activé (conflit de protocole)",
        39 => "Périphérique non disponible (mise à jour du firmware requise)",
        40 => "Périphérique non disponible (réinitialisation requise)",
        41 => "Périphérique non disponible (alimentation insuffisante)",
        42 => "Périphérique non disponible (ressources système insuffisantes)",
        43 => "Périphérique non disponible (en attente de ressources)",
        44 => "Périphérique non disponible (mise à jour du pilote requise)",
        45 => "Périphérique non disponible (sécurité système)",
        46 => "Périphérique non disponible (paramètres de sécurité)",
        47 => "Périphérique non disponible (quota dépassé)",
        _  => "Code d'erreur inconnu"
    };

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

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

    private static int GetInt(ManagementBaseObject mo, string property)
    {
        try
        {
            var val = mo[property];
            if (val is null) return 0;
            if (val is int iv) return iv;
            return Convert.ToInt32(val);
        }
        catch
        {
            return 0;
        }
    }

    private static string[] GetStringArray(ManagementBaseObject mo, string property)
    {
        try { if (mo[property] is string[] arr) return arr; }
        catch { }
        return [];
    }
}
