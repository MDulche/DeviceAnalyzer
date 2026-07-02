using System.Windows;
using Microsoft.Win32;

namespace DeviceAnalyzer.Services;

public class DialogService : IDialogService
{
    public void ShowError(string message, string title = "Erreur")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public string? ShowSaveDialog(string filter, string defaultFileName)
    {
        var dlg = new SaveFileDialog
        {
            Filter = filter,
            FileName = defaultFileName
        };

        return dlg.ShowDialog() == true ? dlg.FileName : null;
    }
}
