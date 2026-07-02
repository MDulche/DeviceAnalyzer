using System.Windows;

namespace DeviceAnalyzer.Services;

public class ClipboardService : IClipboardService
{
    public void CopyText(string text)
    {
        try { Clipboard.SetText(text); }
        catch { }
    }
}
