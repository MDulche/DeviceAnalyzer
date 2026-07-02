# DeviceAnalyzer

**English** | [Français](#français)

## English

A lightweight Windows PnP device analysis tool built with C# .NET 8 / WPF and MVVM architecture.

More readable and diagnostic-focused than the built-in Windows Device Manager.

### Features

- **Real-time monitoring** — detects device connection/disconnection via WMI events with a live timestamped log
- **Category filters** — USB, HID, Audio, Disk, Network, Bluetooth with color-coded badges
- **Advanced sorting** — by name, class, or status (ascending/descending)
- **Error detection** — devices with non-OK status are highlighted in red with error count badge
- **Full details** — name, class, status, manufacturer, PNPDeviceID, InstanceID, HardwareID, driver info
- **Export** — CSV, JSON, and event log export
- **Copy to clipboard** — one-click copy of technical device info
- **Non-blocking UI** — async WMI queries keep the interface responsive
- **Live event log** — timestamped history of device arrivals and removals

### Requirements

- Windows 10 / 11
- [.NET 8 Runtime](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build

```bash
dotnet build DeviceAnalyzer.slnx
```

---

## Français

Outil léger d'analyse des périphériques PnP Windows en C# .NET 8 / WPF, architecture MVVM.

Plus lisible et orienté diagnostic que le Gestionnaire de périphériques Windows.

### Fonctionnalités

- **Surveillance temps réel** — détection branchement/débranchement par événements WMI avec journal horodaté
- **Filtres par catégorie** — USB, HID, Audio, Disque, Réseau, Bluetooth avec badges colorés
- **Tri avancé** — par nom, classe ou statut (croissant/décroissant)
- **Détection d'erreurs** — périphériques non-OK surlignés en rouge avec compteur
- **Détails complets** — nom, classe, statut, fabricant, PNPDeviceID, InstanceID, HardwareID, pilote
- **Export** — CSV, JSON et journal d'événements
- **Copier dans le presse-papiers** — un clic pour copier les infos techniques
- **Interface non bloquante** — requêtes WMI asynchrones
- **Journal live** — historique des connexions/déconnexions

### Prérequis

- Windows 10 / 11
- [Runtime .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0)

### Compilation

```bash
dotnet build DeviceAnalyzer.slnx
```

---

**License:** MIT
