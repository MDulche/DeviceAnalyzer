namespace DeviceAnalyzer.Models;

public enum DeviceEventType
{
    Connected,
    Disconnected
}

public class DeviceLogEntry
{
    public DateTime Timestamp { get; }
    public DeviceEventType EventType { get; }
    public string DeviceName { get; }
    public string PnpDeviceId { get; }
    public string InstanceId { get; }

    public DeviceLogEntry(DeviceEventType eventType, string deviceName, string pnpDeviceId, string instanceId)
    {
        Timestamp = DateTime.Now;
        EventType = eventType;
        DeviceName = string.IsNullOrWhiteSpace(deviceName) ? "(Périphérique inconnu)" : deviceName;
        PnpDeviceId = pnpDeviceId;
        InstanceId = instanceId;
    }

    public string Summary => $"[{Timestamp:HH:mm:ss}] {(EventType == DeviceEventType.Connected ? "Connecté" : "Déconnecté")}  {DeviceName}";
}
