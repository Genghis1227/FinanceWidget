using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;

namespace FinanceWidget
{
    public partial class MainWindow : Window
    {
        public string CurrentTicker { get; private set; }
        public bool UseBetaSite { get; private set; }

        public MainWindow(string ticker = "JEPQ:NASDAQ", double left = double.NaN, double top = double.NaN, bool keepOnTop = true, bool useBetaSite = false)
        {
            InitializeComponent();
            CurrentTicker = ticker;
            UseBetaSite = useBetaSite;

            if (!double.IsNaN(left) && !double.IsNaN(top))
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = left;
                this.Top = top;
            }
            
            this.Topmost = keepOnTop;
            KeepOnTopMenuItem.IsChecked = keepOnTop;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            BrowserContainer.Opacity = 0; // Hide container instead of WebView2 to prevent rendering bugs
            await Browser.EnsureCoreWebView2Async(null);

            // Disable features that show up as hover toolbars/status bar
            Browser.CoreWebView2.Settings.IsStatusBarEnabled = false;
            Browser.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            Browser.CoreWebView2.Settings.IsZoomControlEnabled = false;
            Browser.CoreWebView2.Settings.IsBuiltInErrorPageEnabled = false;

            Browser.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            LoadTicker(CurrentTicker);
        }

        private void LoadTicker(string ticker)
        {
            if (Browser.CoreWebView2 != null)
            {
                BrowserContainer.Opacity = 0; 
                string baseUrl = UseBetaSite ? "https://www.google.com/finance/beta/quote" : "https://www.google.com/finance/quote";
                Browser.CoreWebView2.Navigate($"{baseUrl}/{ticker}");
                TickerLabel.Text = ticker.Split(':')[0];
            }
        }

        private async void CoreWebView2_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (e.IsSuccess && Browser.CoreWebView2 != null)
            {
                string script = @"
                    var style = document.createElement('style');
                    style.innerHTML = `
                        /* Hide headers, footers, search bars, sidebars, and Compare to inputs */
                        header, #gb, footer, aside, div.ZhXxed, div.Y8k45b.fXy5cc, div.fXy5cc, .bF2ive, .F1hUFe, .d73ztd, .qN1Zdf, .N35Hn, .tAEEs, .z4ebah, .PF7Y8c, .id-compare-input, .id-compare-button, .id-compare-input-div, button[aria-label=""Compare to financial entity""] { display: none !important; }
                        
                        /* Basic clean up */
                        body { overflow: hidden !important; background-color: white !important; margin: 0 !important; padding: 0 !important; }
                        ::-webkit-scrollbar { display: none !important; }

                        /* Classic site specific hides */
                        h1 { display: none !important; }
                    `;
                    document.head.appendChild(style);

                    function isolateChart() {
                        // Classic breadcrumb hide
                        var h1 = document.querySelector('h1');
                        if (h1 && h1.previousElementSibling) h1.previousElementSibling.style.display = 'none';

                        // Beta breadcrumb hide
                        var betaBreadcrumb = document.querySelector('a.DJj6Nc');
                        if (betaBreadcrumb && betaBreadcrumb.parentElement) betaBreadcrumb.parentElement.style.display = 'none';

                        // Hide Google top bar spacer (classic)
                        var gb = document.getElementById('gb');
                        if (gb && gb.nextElementSibling && gb.nextElementSibling.tagName === 'DIV') {
                            gb.nextElementSibling.style.display = 'none';
                        }
                        
                        // Hide everything below the chart container (Beta)
                        var chart = document.querySelector('div.Gxz3Gc') || document.querySelector('div.VfPpkd-dgl2Hf-pp98Pc');
                        if (chart) {
                            var sibling = chart.nextElementSibling;
                            while (sibling) {
                                sibling.style.display = 'none';
                                sibling = sibling.nextElementSibling;
                            }
                            // Fallback: hide parent siblings if the chart is wrapped
                            if (chart.parentElement && chart.parentElement.nextElementSibling) {
                                chart.parentElement.nextElementSibling.style.display = 'none';
                            }
                        }

                        // Scroll directly to the price header to chop off any remaining whitespace
                        var price = document.querySelector('div.V837kb') || document.querySelector('.YMlKec.fxKbKc');
                        if (price) {
                            var rect = price.getBoundingClientRect();
                            window.scrollTo(0, rect.top + window.scrollY - 15);
                        } else {
                            window.scrollTo(0, 0);
                        }
                    }
                    isolateChart();
                    setTimeout(isolateChart, 200);
                    setTimeout(isolateChart, 500);
                ";
                await Browser.CoreWebView2.ExecuteScriptAsync(script);

                // Show after applying styles to prevent any flash
                await Task.Delay(300);
                BrowserContainer.Opacity = 1;
            }
        }



        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        private void KeepOnTop_Click(object sender, RoutedEventArgs e)
        {
            Topmost = KeepOnTopMenuItem.IsChecked;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsWindow(CurrentTicker, UseBetaSite);
            if (settings.ShowDialog() == true)
            {
                bool settingsChanged = false;
                if (UseBetaSite != settings.UseBetaSite)
                {
                    UseBetaSite = settings.UseBetaSite;
                    settingsChanged = true;
                }

                if (CurrentTicker != settings.Ticker)
                {
                    CurrentTicker = settings.Ticker;
                    settingsChanged = true;
                }

                if (settingsChanged)
                {
                    LoadTicker(CurrentTicker);
                }
            }
        }



        private void CloseWidget_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}