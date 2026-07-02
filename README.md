# DeviceAnalyzer

**English** | [Français](#français)

Lightweight PnP device diagnostic tool for Windows — faster and more insightful than the built-in Device Manager.

Built with C# .NET 8 / WPF, MVVM architecture. Self-contained executable, no runtime required.

---

## English

### Features

**Real-time monitoring**
- WMI event watchers detect device connect/disconnect instantly
- Live timestamped event log with deduplication
- Auto-refresh with 1s debounce to avoid storms

**Advanced diagnostics**
- **ConfigManager error codes** — exact error code (0–47) with French description for every device
- **Per-device event history** — tracks connect/disconnect count per device during the session
- **Instability detection** — devices with 3+ disconnections flagged as unstable
- **Error & problem filters** — toggle between errors only, problems only, or all devices
- **New device highlight** — newly detected devices glow green for 5 seconds

**Filtering & sorting**
- Category filter: USB, HID, Audio, Disk, Network, Bluetooth
- Sort by name, class, or status (ascending/descending)
- Free-text search across 6 fields (name, class, manufacturer, PNPDeviceID, InstanceID, HardwareID)

**Detail panel** (sectioned)
- Identité, Statut, Matériel, Pilote, Événements
- Color-coded status and error codes
- One-click copy to clipboard

**Export**
- CSV, JSON, or plain-text event log
- All exports include the ConfigManager error code

### Quick start

**Option 1 — Download the pre-built exe**
Grab `DeviceAnalyzer-win64.zip` from the repo root. Extract and run `DeviceAnalyzer.exe`. No dependencies required.

**Option 2 — Build from source**
Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/MDulche/DeviceAnalyzer.git
cd DeviceAnalyzer
dotnet build DeviceAnalyzer.slnx
```

**Self-contained publish** (single-file exe, ~162 MB):

```bash
dotnet publish DeviceAnalyzer/DeviceAnalyzer.csproj ^
  -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true
```

The exe will be at `DeviceAnalyzer/bin/Release/net8.0-windows/win-x64/publish/DeviceAnalyzer.exe`.

### Requirements

- Windows 10 / 11 (64-bit)

---

## Français

### Fonctionnalités

**Surveillance temps réel**
- Détection branchement/débranchement instantanée par événements WMI
- Journal dédupliqué avec horodatage
- Rechargement automatique avec debounce 1s

**Diagnostic avancé**
- **Codes d'erreur ConfigManager** — code précis (0–47) avec description française
- **Historique par périphérique** — compteur de connexions/déconnexions pendant la session
- **Détection d'instabilité** — périphériques avec ≥3 déconnexions signalés
- **Filtres Erreurs & Problèmes** — bascule entre erreurs seulement, problèmes seulement, ou tout
- **Nouveautés** — les nouveaux périphériques sont surlignés en vert 5 secondes

**Filtrage & tri**
- Filtre par catégorie : USB, HID, Audio, Disque, Réseau, Bluetooth
- Tri par nom, classe ou statut (croissant/décroissant)
- Recherche libre sur 6 champs (nom, classe, fabricant, PNPDeviceID, InstanceID, HardwareID)

**Panneau de détail** (sectionné)
- Identité, Statut, Matériel, Pilote, Événements
- Statut et codes d'erreur colorés
- Copie en un clic

**Export**
- CSV, JSON ou journal texte
- Tous les exports incluent le code d'erreur ConfigManager

### Utilisation rapide

**Option 1 — Exécutable pré-compilé**
Téléchargez `DeviceAnalyzer-win64.zip` à la racine du dépôt. Extrayez et lancez `DeviceAnalyzer.exe`. Aucune dépendance.

**Option 2 — Compilation**
Nécessite [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```bash
git clone https://github.com/MDulche/DeviceAnalyzer.git
cd DeviceAnalyzer
dotnet build DeviceAnalyzer.slnx
```

**Publication autonome** (exe mono-fichier, ~162 Mo) :

```bash
dotnet publish DeviceAnalyzer/DeviceAnalyzer.csproj ^
  -c Release -r win-x64 --self-contained true ^
  -p:PublishSingleFile=true
```

L'exe sera dans `DeviceAnalyzer/bin/Release/net8.0-windows/win-x64/publish/DeviceAnalyzer.exe`.

### Prérequis

- Windows 10 / 11 (64-bit)

---

## License

MIT
