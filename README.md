# Finance Widget (v0.2)

A lightweight, modern desktop application for Windows that displays clean, borderless stock and ETF charts.

## Features
- **Multi-Widget Support:** Spawn as many independent widgets as you want.
- **System Tray Integration:** Runs quietly in the background without cluttering your taskbar.
- **State Persistence:** Automatically remembers your chosen ticker symbols, window positions on your screen, and "Keep on Top" preferences across restarts.
- **Clean UI:** Injects custom CSS to strip away headers, footers, and sidebars from Google Finance, leaving just the chart and price.
- **Beta UI Support (Experimental):** Toggle the use of the new Google Finance Beta interface directly from the widget settings.
- **Keep on Top:** Pin individual widgets to always stay above other windows.

## Requirements
To run the pre-built single-file executable, you only need:
- Windows 10 or 11
- [Microsoft Edge WebView2 Runtime](https://developer.microsoft.com/en-us/microsoft-edge/webview2/) (Pre-installed on Windows 11 and most Windows 10 systems)

## Building & Deploying
This project is built using WPF and .NET 10. To package the application into a single, self-contained executable that you can easily share with others (no .NET installation required on their end), run the following command in the project directory:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true
```

The resulting executable will be placed in:
`bin\Release\net10.0-windows\win-x64\publish\FinanceWidget.exe`

## Usage
1. Launch `FinanceWidget.exe`.
2. Find the green icon in your System Tray (bottom right of your screen, near the clock).
3. **Add Additional Widgets:** Right-click the tray icon and select **"Add New Widget"** to spawn as many independent trackers as you want.
4. **Change the Ticker:** Right-click any active widget and click **"Settings"**. 
5. **Ticker Format:** When in Settings, you must enter the symbol in standard Google Finance format (`TICKER:EXCHANGE`). For example:
   - `AAPL:NASDAQ` (Apple on the NASDAQ)
   - `VOO:NYSEARCA` (Vanguard S&P 500 ETF on NYSE Arca)
   - `JEPQ:NASDAQ` (JPMorgan Nasdaq Equity Premium Income ETF)
6. **Move & Organize:** Drag any widget by its top gray drag-handle to move it anywhere on your screen.
7. **Keep on Top:** Right-click any active widget and toggle **"Keep on Top"** to pin it above other windows.
8. **Exit:** Right-click the system tray icon and select **"Exit All"** to close all widgets and shut down the app completely.
