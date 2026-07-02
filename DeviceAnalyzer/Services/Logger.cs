using System.Diagnostics;

namespace DeviceAnalyzer.Services;

public class Logger : ILogger
{
    public void Info(string message) => Debug.WriteLine($"[INFO] {DateTime.Now:HH:mm:ss} {message}");
    public void Warn(string message) => Debug.WriteLine($"[WARN] {DateTime.Now:HH:mm:ss} {message}");
    public void Error(string message, Exception? ex = null)
    {
        Debug.WriteLine($"[ERROR] {DateTime.Now:HH:mm:ss} {message}");
        if (ex is not null)
            Debug.WriteLine($"  {ex}");
    }
}
