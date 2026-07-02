using System.Management;
using System.Text;
using System.Text.Json;
using DeviceAnalyzer.Models;

namespace DeviceAnalyzer.Services;

public class PnpDeviceService
{
    public List<PnpDevice> GetAllDevices()
    {
        var devices = new List<PnpDevice>();

        using var searcher = new ManagementObjectSearcher(
            "SELECT * FROM Win32_PnPEntity");

        foreach (var mo in searcher.Get())
            devices.Add(new PnpDevice(mo));

        return devices;
    }

    public string ExportToCsv(List<PnpDevice> devices)
    {
        var sb = new StringBuilder();
        sb.AppendLine("Nom;Classe;Statut;Fabricant;PNPDeviceID;InstanceID;HardwareID;Description;VersionPilote;DatePilote;Present;Categorie");

        foreach (var d in devices)
        {
            sb.AppendLine($"{EscapeCsv(d.Name)};{EscapeCsv(d.Class)};{EscapeCsv(d.Status)};{EscapeCsv(d.Manufacturer)};{EscapeCsv(d.PnpDeviceId)};{EscapeCsv(d.InstanceId)};{EscapeCsv(d.HardwareId)};{EscapeCsv(d.Description)};{EscapeCsv(d.DriverVersion)};{EscapeCsv(d.DriverDate)};{d.IsPresent};{d.Category}");
        }

        return sb.ToString();
    }

    public string ExportToJson(List<PnpDevice> devices)
    {
        var data = devices.Select(d => new
        {
            d.Name, d.Class, d.Status, d.Manufacturer,
            PnpDeviceId = d.PnpDeviceId, InstanceId = d.InstanceId,
            d.HardwareId, d.Description, DriverVersion = d.DriverVersion,
            DriverDate = d.DriverDate, d.IsPresent,
            Categorie = d.Category.ToString()
        });

        return JsonSerializer.Serialize(data, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });
    }

    public string ExportLog(List<DeviceLogEntry> entries)
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== Journal des événements PnP ===");
        sb.AppendLine($"Généré le : {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        foreach (var e in entries)
        {
            var type = e.EventType == DeviceEventType.Connected ? "CONNECTÉ" : "DÉCONNECTÉ";
            sb.AppendLine($"[{e.Timestamp:yyyy-MM-dd HH:mm:ss}] {type}  {e.DeviceName}");
            sb.AppendLine($"    PNPDeviceID : {e.PnpDeviceId}");
            sb.AppendLine($"    InstanceID  : {e.InstanceId}");
            sb.AppendLine();
        }

        return sb.ToString();
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return "";
        if (value.Contains(';') || value.Contains('"') || value.Contains('\n'))
            return $"\"{value.Replace("\"", "\"\"")}\"";

        return value;
    }
}
