using DeviceAnalyzer.Models;

namespace DeviceAnalyzer.Services;

public interface IExportService
{
    void ExportCsv(IReadOnlyList<PnpDevice> devices);
    void ExportJson(IReadOnlyList<PnpDevice> devices);
    void ExportLog(IReadOnlyList<DeviceLogEntry> entries);
}
