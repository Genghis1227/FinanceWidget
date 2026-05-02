using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace FinanceWidget
{
    public partial class App : Application
    {
        public static string Version => "0.9.0";
        private TaskbarIcon? _taskbarIcon;
        private string _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FinanceWidget", "appsettings.json");
        private const string GitHubRepoUrl = "https://github.com/Genghis1227/FinanceWidget";
        private const string GitHubApiUrl = "https://api.github.com/repos/Genghis1227/FinanceWidget/releases/latest";

        private bool _isShuttingDown = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Keep app alive even if all widgets are closed
            ShutdownMode = ShutdownMode.OnExplicitShutdown;

            // Initialize Tray Icon
            _taskbarIcon = new TaskbarIcon
            {
                IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/icon.png")),
                ToolTipText = "Finance Widget Manager"
            };

            _taskbarIcon.TrayMouseDoubleClick += (s, args) => BringWidgetsToFront();

            // Build Tray Context Menu
            var contextMenu = new ContextMenu();
            
            var addWidgetMenuItem = new MenuItem { Header = "Add New Widget" };
            addWidgetMenuItem.Click += (s, args) => SpawnNewWidget();
            
            var releaseNotesMenuItem = new MenuItem { Header = "Release Notes" };
            releaseNotesMenuItem.Click += (s, args) => OpenUrl($"{GitHubRepoUrl}/blob/main/ReleaseNotes/release_notes_v{Version}.md");

            var checkUpdatesMenuItem = new MenuItem { Header = "Check for Updates" };
            checkUpdatesMenuItem.Click += async (s, args) => await CheckForUpdatesAsync(true);

            var exitMenuItem = new MenuItem { Header = "Exit All" };
            exitMenuItem.Click += (s, args) => 
            {
                _isShuttingDown = true;
                SaveState();
                Current.Shutdown();
            };

            contextMenu.Items.Add(addWidgetMenuItem);
            contextMenu.Items.Add(releaseNotesMenuItem);
            contextMenu.Items.Add(checkUpdatesMenuItem);
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(exitMenuItem);

            _taskbarIcon.ContextMenu = contextMenu;

            // Load saved state or spawn a default widget
            LoadState();
        }

        private void LoadState()
        {
            if (File.Exists(_settingsFilePath))
            {
                try
                {
                    string json = File.ReadAllText(_settingsFilePath);
                    var state = JsonSerializer.Deserialize<AppState>(json);

                    if (state != null && state.Widgets != null && state.Widgets.Count > 0)
                    {
                        foreach (var config in state.Widgets)
                        {
                            var widget = new MainWindow(config.Ticker, config.Left, config.Top, config.Width, config.Height, config.KeepOnTop, config.UseBetaSite);
                            widget.Show();
                        }
                        return;
                    }
                }
                catch
                {
                    // If error parsing, ignore and fallback to default
                }
            }

            // Fallback to default if no widgets exist
            SpawnNewWidget(isFirstRun: true);
        }

        private void SaveState()
        {
            var state = new AppState();
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow mw)
                {
                    state.Widgets.Add(new WidgetConfig
                    {
                        Ticker = mw.CurrentTicker,
                        Left = mw.Left,
                        Top = mw.Top,
                        Width = mw.ActualWidth,
                        Height = mw.ActualHeight,
                        KeepOnTop = mw.Topmost,
                        UseBetaSite = mw.UseBetaSite
                    });
                }
            }

            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
                string json = JsonSerializer.Serialize(state, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_settingsFilePath, json);
            }
            catch
            {
                // Ignore save errors
            }
        }

        public void SpawnNewWidgetFromTicker(string ticker)
        {
            if (!string.IsNullOrEmpty(ticker))
            {
                var widget = new MainWindow(ticker);
                widget.Show();
            }
        }

        public void SpawnNewWidget(bool isFirstRun = false)
        {
            var prompt = new AddTickerWindow();
            
            // If it's the first run, we might want to center it better or provide different context
            // But for now, just showing the dialog is a big improvement over a silent default.
            
            if (prompt.ShowDialog() == true)
            {
                string ticker = prompt.Ticker;
                if (!string.IsNullOrEmpty(ticker))
                {
                    var widget = new MainWindow(ticker);
                    widget.Show();
                }
            }
            else if (isFirstRun)
            {
                // If user cancels on first run, and there are no windows, we should probably 
                // either exit or show a default so the app isn't just a hidden tray icon.
                // However, the user said they DON'T want the default JEPQ.
                // Let's just exit if they cancel the very first prompt and nothing else is open.
                if (Application.Current.Windows.Count == 0)
                {
                    Current.Shutdown();
                }
            }
        }

        private void BringWidgetsToFront()
        {
            foreach (Window window in Application.Current.Windows)
            {
                if (window is MainWindow)
                {
                    if (window.WindowState == WindowState.Minimized)
                    {
                        window.WindowState = WindowState.Normal;
                    }
                    window.Activate();
                    window.Focus();
                }
            }
        }

        public async Task CheckForUpdatesAsync(bool manualCheck)
        {
            try
            {
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "FinanceWidget-Updater");
                var response = await client.GetStringAsync(GitHubApiUrl);
                var doc = JsonDocument.Parse(response);
                var latestVersionTag = doc.RootElement.GetProperty("tag_name").GetString()?.Replace("v", "") ?? "";

                if (IsNewerVersion(latestVersionTag, Version))
                {
                    var result = MessageBox.Show(
                        $"A new version (v{latestVersionTag}) is available!\n\nWould you like to go to the download page?",
                        "Update Available",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        OpenUrl($"{GitHubRepoUrl}/releases/latest");
                    }
                }
                else if (manualCheck)
                {
                    MessageBox.Show(
                        $"You are running the latest version (v{Version}).",
                        "No Updates Found",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                if (manualCheck)
                {
                    MessageBox.Show($"Error checking for updates: {ex.Message}", "Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private bool IsNewerVersion(string latest, string current)
        {
            if (System.Version.TryParse(latest, out var latestVer) && System.Version.TryParse(current, out var currentVer))
            {
                return latestVer > currentVer;
            }
            return false;
        }

        public void OpenUrl(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (!_isShuttingDown)
            {
                SaveState();
            }
            _taskbarIcon?.Dispose();
            base.OnExit(e);
        }
    }
}
