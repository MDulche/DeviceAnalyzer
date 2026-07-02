using System.Management;

namespace DeviceAnalyzer.Services;

public class DeviceEventMonitor : IDisposable
{
    private ManagementEventWatcher? _creationWatcher;
    private ManagementEventWatcher? _deletionWatcher;
    private readonly ILogger _logger;

    public event Action<string, string, string>? DeviceConnected;
    public event Action<string, string, string>? DeviceDisconnected;

    public DeviceEventMonitor(ILogger logger)
    {
        _logger = logger;
    }

    public void Start()
    {
        var creationQuery = new WqlEventQuery(
            "__InstanceCreationEvent",
            TimeSpan.FromSeconds(1),
            "TargetInstance ISA 'Win32_PnPEntity'");

        _creationWatcher = new ManagementEventWatcher(creationQuery);
        _creationWatcher.EventArrived += OnCreationEvent;
        _creationWatcher.Start();

        var deletionQuery = new WqlEventQuery(
            "__InstanceDeletionEvent",
            TimeSpan.FromSeconds(1),
            "TargetInstance ISA 'Win32_PnPEntity'");

        _deletionWatcher = new ManagementEventWatcher(deletionQuery);
        _deletionWatcher.EventArrived += OnDeletionEvent;
        _deletionWatcher.Start();

        _logger.Info("DeviceEventMonitor démarré");
    }

    private void OnCreationEvent(object? sender, EventArrivedEventArgs e)
    {
        try
        {
            if (e.NewEvent["TargetInstance"] is ManagementBaseObject instance)
            {
                var name = GetString(instance, "Name");
                var pnpId = GetString(instance, "PNPDeviceID");
                var devId = GetString(instance, "DeviceID");
                DeviceConnected?.Invoke(name, pnpId, devId);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Erreur dans OnCreationEvent", ex);
        }
    }

    private void OnDeletionEvent(object? sender, EventArrivedEventArgs e)
    {
        try
        {
            if (e.NewEvent["TargetInstance"] is ManagementBaseObject instance)
            {
                var name = GetString(instance, "Name");
                var pnpId = GetString(instance, "PNPDeviceID");
                var devId = GetString(instance, "DeviceID");
                DeviceDisconnected?.Invoke(name, pnpId, devId);
            }
        }
        catch (Exception ex)
        {
            _logger.Error("Erreur dans OnDeletionEvent", ex);
        }
    }

    private static string GetString(ManagementBaseObject mo, string property)
    {
        try { return mo[property]?.ToString()?.Trim() ?? ""; }
        catch { return ""; }
    }

    public void Dispose()
    {
        _creationWatcher?.Stop();
        _creationWatcher?.Dispose();
        _deletionWatcher?.Stop();
        _deletionWatcher?.Dispose();
        _logger.Info("DeviceEventMonitor arrêté");
    }
}
