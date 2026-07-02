namespace DeviceAnalyzer.Services;

public interface IDialogService
{
    void ShowError(string message, string title = "Erreur");
    string? ShowSaveDialog(string filter, string defaultFileName);
}
