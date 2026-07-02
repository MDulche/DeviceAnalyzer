using System.IO;
using System.Text;
using DeviceAnalyzer.Models;

namespace DeviceAnalyzer.Services;

public class ExportService : IExportService
{
    private readonly IDialogService _dialog;
    private readonly PnpDeviceService _formatter = new();

    public ExportService(IDialogService dialog)
    {
        _dialog = dialog;
    }

    public void ExportCsv(IReadOnlyList<PnpDevice> devices)
    {
        var path = _dialog.ShowSaveDialog(
            "Fichier CSV (*.csv)|*.csv",
            $"peripheriques_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

        if (path is null) return;

        try
        {
            var csv = _formatter.ExportToCsv([.. devices]);
            File.WriteAllText(path, csv, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _dialog.ShowError($"Erreur lors de l'export CSV :\n{ex.Message}");
        }
    }

    public void ExportJson(IReadOnlyList<PnpDevice> devices)
    {
        var path = _dialog.ShowSaveDialog(
            "Fichier JSON (*.json)|*.json",
            $"peripheriques_{DateTime.Now:yyyyMMdd_HHmmss}.json");

        if (path is null) return;

        try
        {
            var json = _formatter.ExportToJson([.. devices]);
            File.WriteAllText(path, json, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _dialog.ShowError($"Erreur lors de l'export JSON :\n{ex.Message}");
        }
    }

    public void ExportLog(IReadOnlyList<DeviceLogEntry> entries)
    {
        var path = _dialog.ShowSaveDialog(
            "Fichier texte (*.log;*.txt)|*.log;*.txt",
            $"journal_pnp_{DateTime.Now:yyyyMMdd_HHmmss}.log");

        if (path is null) return;

        try
        {
            var log = _formatter.ExportLog([.. entries]);
            File.WriteAllText(path, log, Encoding.UTF8);
        }
        catch (Exception ex)
        {
            _dialog.ShowError($"Erreur lors de l'export du journal :\n{ex.Message}");
        }
    }
}
