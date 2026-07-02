using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using DeviceAnalyzer.Helpers;
using DeviceAnalyzer.Models;
using DeviceAnalyzer.Services;

namespace DeviceAnalyzer.ViewModels;

public class MainViewModel : INotifyPropertyChanged, IDisposable
{
    private readonly PnpDeviceService _service = new();
    private readonly DeviceEventMonitor _monitor;
    private readonly ILogger _logger;
    private readonly IClipboardService _clipboard;
    private readonly IExportService _export;
    private readonly IDialogService _dialog;
    private readonly DispatcherTimer _highlightTimer = new();
    private readonly DispatcherTimer _eventDebounce = new();

    private CancellationTokenSource? _refreshCts;
    private List<PnpDevice> _allDevicesCache = [];
    private HashSet<string> _knownDeviceIds = [];

    private PnpDevice? _selectedDevice;
    private string _searchText = "";
    private bool _isLoading;
    private DeviceCategory _selectedCategory = DeviceCategory.All;
    private string _sortField = "Nom";
    private bool _sortAscending = true;
    private bool _showErrorsOnly;
    private string _summaryText = "";

    public ObservableCollection<PnpDevice> Devices { get; } = [];
    public ObservableCollection<DeviceLogEntry> LogEntries { get; } = [];

    public ICommand RefreshCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand ClearLogCommand { get; }
    public ICommand CopyDeviceInfoCommand { get; }
    public ICommand ExportCsvCommand { get; }
    public ICommand ExportJsonCommand { get; }
    public ICommand ExportLogCommand { get; }
    public ICommand SetCategoryCommand { get; }

    public List<string> SortFields { get; } = ["Nom", "Classe", "Statut"];

    public MainViewModel()
        : this(new Logger(), new ClipboardService(), new ExportService(new DialogService()), new DialogService())
    {
    }

    public MainViewModel(ILogger logger, IClipboardService clipboard, IExportService export, IDialogService dialog)
    {
        _logger = logger;
        _clipboard = clipboard;
        _export = export;
        _dialog = dialog;

        RefreshCommand = new RelayCommand(_ => _ = LoadDevicesAsync());
        ClearSearchCommand = new RelayCommand(_ => SearchText = "");
        ClearLogCommand = new RelayCommand(_ => LogEntries.Clear());
        CopyDeviceInfoCommand = new RelayCommand(_ => CopyDeviceInfo(), _ => SelectedDevice is not null);
        ExportCsvCommand = new RelayCommand(_ => _export.ExportCsv(Devices));
        ExportJsonCommand = new RelayCommand(_ => _export.ExportJson(Devices));
        ExportLogCommand = new RelayCommand(_ => _export.ExportLog(LogEntries));
        SetCategoryCommand = new RelayCommand(p =>
        {
            if (p is string s && Enum.TryParse<DeviceCategory>(s, out var cat))
                SelectedCategory = cat;
        });

        _highlightTimer.Interval = TimeSpan.FromSeconds(5);
        _highlightTimer.Tick += OnHighlightTimerTick;

        _eventDebounce.Interval = TimeSpan.FromMilliseconds(300);
        _eventDebounce.Tick += OnEventDebounceTick;

        _monitor = new DeviceEventMonitor(_logger);
        _monitor.DeviceConnected += OnDeviceConnected;
        _monitor.DeviceDisconnected += OnDeviceDisconnected;
        _monitor.Start();

        _logger.Info("MainViewModel initialisé");
        _ = LoadDevicesAsync();
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); }
    }

    public PnpDevice? SelectedDevice
    {
        get => _selectedDevice;
        set { _selectedDevice = value; OnPropertyChanged(); OnPropertyChanged(nameof(CanCopy)); }
    }

    public bool CanCopy => SelectedDevice is not null;

    public string SummaryText
    {
        get => _summaryText;
        set { _summaryText = value; OnPropertyChanged(); }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText != value)
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public DeviceCategory SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            if (_selectedCategory != value)
            {
                _selectedCategory = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public string SortField
    {
        get => _sortField;
        set
        {
            if (_sortField != value)
            {
                _sortField = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public bool SortAscending
    {
        get => _sortAscending;
        set
        {
            if (_sortAscending != value)
            {
                _sortAscending = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public bool ShowErrorsOnly
    {
        get => _showErrorsOnly;
        set
        {
            if (_showErrorsOnly != value)
            {
                _showErrorsOnly = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }
    }

    public int DeviceCount => Devices.Count;
    public int LogCount => LogEntries.Count;
    public int ErrorCount => Devices.Count(d => d.IsError);

    private async Task LoadDevicesAsync()
    {
        _refreshCts?.Cancel();
        _refreshCts = new CancellationTokenSource();
        var token = _refreshCts.Token;

        IsLoading = true;

        try
        {
            _allDevicesCache = await Task.Run(() => _service.GetAllDevices(), token);
            token.ThrowIfCancellationRequested();

            _logger.Info($"Cache rechargé : {_allDevicesCache.Count} périphériques");
            ApplyFilters();
        }
        catch (OperationCanceledException)
        {
            _logger.Info("Rechargement annulé");
        }
        catch (Exception ex)
        {
            _logger.Error("Erreur lors du chargement des périphériques", ex);
            _dialog.ShowError($"Erreur lors du chargement des périphériques :\n{ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplyFilters()
    {
        var previousIds = _knownDeviceIds;
        var newIds = new HashSet<string>();
        var savedSelectionId = SelectedDevice?.PnpDeviceId;

        var list = _allDevicesCache.AsEnumerable();

        if (_selectedCategory != DeviceCategory.All)
            list = list.Where(d => d.Category == _selectedCategory);

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var f = _searchText.Trim();
            list = list.Where(d =>
                d.Name.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                d.Class.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                d.Manufacturer.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                d.PnpDeviceId.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                d.InstanceId.Contains(f, StringComparison.OrdinalIgnoreCase) ||
                d.HardwareId.Contains(f, StringComparison.OrdinalIgnoreCase));
        }

        if (_showErrorsOnly)
            list = list.Where(d => d.IsError);

        list = _sortField switch
        {
            "Classe" => _sortAscending
                ? list.OrderBy(d => d.Class).ThenBy(d => d.DisplayName)
                : list.OrderByDescending(d => d.Class).ThenBy(d => d.DisplayName),
            "Statut" => _sortAscending
                ? list.OrderBy(d => d.Status == "OK" ? 0 : 1).ThenBy(d => d.DisplayName)
                : list.OrderByDescending(d => d.Status == "OK" ? 0 : 1).ThenBy(d => d.DisplayName),
            _ => _sortAscending
                ? list.OrderBy(d => d.DisplayName)
                : list.OrderByDescending(d => d.DisplayName)
        };

        var filtered = list.ToList();

        Devices.Clear();

        foreach (var d in filtered)
        {
            newIds.Add(d.PnpDeviceId);
            if (!previousIds.Contains(d.PnpDeviceId))
                d.HighlightState = HighlightState.New;
            Devices.Add(d);
        }

        _knownDeviceIds = newIds;

        if (Devices.Any(d => d.HighlightState == HighlightState.New))
            _highlightTimer.Start();

        // Restore selection
        if (savedSelectionId is not null)
        {
            SelectedDevice = Devices.FirstOrDefault(d => d.PnpDeviceId == savedSelectionId);
        }

        var total = Devices.Count;
        var ok = Devices.Count(d => d.Status == "OK");
        var err = Devices.Count(d => d.IsError);

        SummaryText = $"{total} périphérique{(total > 1 ? "s" : "")}  ·  ✅ {ok} OK  ·  ❌ {err} erreur{(err > 1 ? "s" : "")}";

        OnPropertyChanged(nameof(DeviceCount));
        OnPropertyChanged(nameof(ErrorCount));
    }

    private void OnDeviceConnected(string name, string pnpId, string instanceId)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogEntries.Insert(0, new DeviceLogEntry(DeviceEventType.Connected, name, pnpId, instanceId));
            OnPropertyChanged(nameof(LogCount));
            _logger.Info($"Événement connecté : {name} ({pnpId})");
            _eventDebounce.Stop();
            _eventDebounce.Start();
        });
    }

    private void OnDeviceDisconnected(string name, string pnpId, string instanceId)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            LogEntries.Insert(0, new DeviceLogEntry(DeviceEventType.Disconnected, name, pnpId, instanceId));
            OnPropertyChanged(nameof(LogCount));
            _logger.Info($"Événement déconnecté : {name} ({pnpId})");
            _eventDebounce.Stop();
            _eventDebounce.Start();
        });
    }

    private void OnEventDebounceTick(object? sender, EventArgs e)
    {
        _eventDebounce.Stop();
        _logger.Info("Debounce déclenché → rechargement");
        _ = LoadDevicesAsync();
    }

    private void OnHighlightTimerTick(object? sender, EventArgs e)
    {
        _highlightTimer.Stop();
        foreach (var d in _allDevicesCache)
            d.HighlightState = HighlightState.None;
        ApplyFilters();
    }

    private void CopyDeviceInfo()
    {
        if (SelectedDevice is null) return;

        var d = SelectedDevice;
        var text = $"Nom          : {d.DisplayName}\n" +
                   $"Catégorie    : {d.Category}\n" +
                   $"Description  : {d.Description}\n" +
                   $"Classe       : {d.Class}\n" +
                   $"Statut       : {d.Status}\n" +
                   $"Fabricant    : {d.Manufacturer}\n" +
                   $"PNPDeviceID  : {d.PnpDeviceId}\n" +
                   $"InstanceID   : {d.InstanceId}\n" +
                   $"HardwareID   : {d.HardwareId}\n" +
                   $"Pilote       : {d.DriverVersion} ({d.DriverDate})\n" +
                   $"Présent      : {(d.IsPresent ? "Oui" : "Non")}";

        _clipboard.CopyText(text);
        _logger.Info($"Copié dans le presse-papiers : {d.DisplayName}");
    }

    public void Dispose()
    {
        _refreshCts?.Cancel();
        _refreshCts?.Dispose();

        _highlightTimer.Stop();
        _highlightTimer.Tick -= OnHighlightTimerTick;

        _eventDebounce.Stop();
        _eventDebounce.Tick -= OnEventDebounceTick;

        _monitor.DeviceConnected -= OnDeviceConnected;
        _monitor.DeviceDisconnected -= OnDeviceDisconnected;
        _monitor.Dispose();

        _logger.Info("MainViewModel disposed");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
