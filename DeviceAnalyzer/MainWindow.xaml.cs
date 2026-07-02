using System.ComponentModel;
using System.Windows;
using DeviceAnalyzer.ViewModels;

namespace DeviceAnalyzer;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        if (DataContext is MainViewModel vm)
            vm.Dispose();

        base.OnClosing(e);
    }
}
