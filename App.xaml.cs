using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Drawing;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace FinanceWidget
{
    public partial class App : Application
    {
        private TaskbarIcon? _taskbarIcon;
        private string _settingsFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FinanceWidget", "appsettings.json");

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
            
            var exitMenuItem = new MenuItem { Header = "Exit All" };
            exitMenuItem.Click += (s, args) => 
            {
                _isShuttingDown = true;
                SaveState();
                Current.Shutdown();
            };

            contextMenu.Items.Add(addWidgetMenuItem);
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
                            var widget = new MainWindow(config.Ticker, config.Left, config.Top, config.KeepOnTop, config.UseBetaSite);
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

            // Fallback to default
            SpawnNewWidget();
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

        private void SpawnNewWidget()
        {
            var widget = new MainWindow();
            widget.Show();
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
