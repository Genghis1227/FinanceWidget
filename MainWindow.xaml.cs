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
                string currentUrl = Browser.CoreWebView2.Source ?? string.Empty;

                // If navigated away from Finance (e.g. to Google login page),
                // show the page as-is so the user can interact with it normally.
                if (!currentUrl.Contains("google.com/finance"))
                {
                    ReturnToFinanceMenuItem.Visibility = Visibility.Visible;
                    BrowserContainer.Opacity = 1;
                    return;
                }

                // Back on Finance — hide the Return to Finance menu item
                ReturnToFinanceMenuItem.Visibility = Visibility.Collapsed;
                string script = @"
                    var style = document.createElement('style');
                    style.textContent = `
                        header, nav, #gb, footer, div.ZhXxed, div.Y8k45b.fXy5cc, div.fXy5cc, .bF2ive, .F1hUFe, .d73ztd, .qN1Zdf, .N35Hn, .tAEEs, .z4ebah, .PF7Y8c, .id-compare-input, .id-compare-button, .id-compare-input-div, .f7t46c, .I6776b { display: none !important; }
                        body { background: white !important; margin: 0 !important; padding: 0 !important; }
                        ::-webkit-scrollbar { display: none !important; }
                    `;
                    document.head.appendChild(style);

                    function isolateChart() {
                        var price = document.querySelector('.YMlS1d') || document.querySelector('div.V837kb') || document.querySelector('.YMlKec.fxKbKc');
                        var chart = document.querySelector('canvas');

                        // Find the STOCK TITLE h1 — skip the Finance logo h1
                        var allH1s = document.querySelectorAll('h1');
                        var h1 = null;
                        for (var qi = 0; qi < allH1s.length; qi++) {
                            var t = (allH1s[qi].textContent || '').trim();
                            if (t.length > 5 && t !== 'Finance') { h1 = allH1s[qi]; break; }
                        }

                        function safeHide(el) {
                            if (!el || el.tagName === 'BODY' || el.tagName === 'HTML') return;
                            if (price && el.contains(price)) return;
                            if (chart && el.contains(chart)) return;
                            if (h1 && el.contains(h1)) return;  // never hide the stock title
                            el.style.setProperty('display', 'none', 'important');
                        }

                        // Kill position:fixed and position:sticky overlays (the Research panel)
                        // Only runs when price is found so we know the quote page loaded correctly
                        if (price) {
                            var allEls = document.querySelectorAll('body > *, body > * > *');
                            for (var i = 0; i < allEls.length; i++) {
                                var cs = window.getComputedStyle(allEls[i]);
                                if (cs.position === 'fixed' || cs.position === 'sticky') {
                                    safeHide(allEls[i]);
                                }
                            }
                        }

                        // Hide known unwanted sections by heading text
                        var badTexts = ['research', 'search for stocks', 'search or ask a question', 'build a watchlist', 'you may be interested in', 'discover more', 'compare markets'];
                        var candidates = document.querySelectorAll('h2, h3');
                        for (var i = 0; i < candidates.length; i++) {
                            var txt = (candidates[i].textContent || '').trim().toLowerCase();
                            for (var t = 0; t < badTexts.length; t++) {
                                if (txt === badTexts[t]) {
                                    var section = candidates[i].closest('section') || candidates[i].closest('[class]');
                                    safeHide(section || candidates[i].parentElement);
                                    break;
                                }
                            }
                        }

                        // Hide the Compare to button (classic site) by text content
                        var allBtns = document.querySelectorAll('button, a');
                        for (var i = 0; i < allBtns.length; i++) {
                            var txt = (allBtns[i].textContent || '').trim().toLowerCase();
                            if (txt.includes('compare to')) {
                                var p = allBtns[i];
                                for (var k = 0; k < 4 && p.parentElement && p.parentElement.tagName !== 'BODY'; k++) p = p.parentElement;
                                safeHide(p);
                            }
                        }

                        // Hide search inputs
                        var inputs = document.querySelectorAll('input[placeholder], textarea[placeholder]');
                        for (var i = 0; i < inputs.length; i++) {
                            var ph = (inputs[i].placeholder || '').toLowerCase();
                            if (ph.includes('search') || ph.includes('ask')) {
                                var p = inputs[i];
                                for (var k = 0; k < 6 && p.parentElement && p.parentElement.tagName !== 'BODY'; k++) p = p.parentElement;
                                safeHide(p);
                            }
                        }

                        // Hide all elements before h1 in its parent (breadcrumb, spacer, action buttons)
                        if (h1) {
                            var sib = h1.previousElementSibling;
                            while (sib) {
                                sib.style.setProperty('display', 'none', 'important');
                                sib = sib.previousElementSibling;
                            }
                        }
                        // Also target Beta breadcrumb link class
                        var breadcrumb = document.querySelector('a.DJj6Nc');
                        if (breadcrumb && breadcrumb.parentElement) {
                            breadcrumb.parentElement.style.setProperty('display', 'none', 'important');
                        }

                        // Directly lock scroll position — the whitespace above the stock
                        // content is ~65px (the Finance header bar height). Setting scrollTop
                        // on both documentElement and body covers all scroll container cases.
                        document.documentElement.scrollTop = 65;
                        document.body.scrollTop = 65;
                    }
                    isolateChart();
                    setInterval(isolateChart, 300);
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



        private void LoginToGoogle_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to Google login; the continue= param redirects back to Finance after login
            string returnUrl = Uri.EscapeDataString($"https://www.google.com/finance/quote/{CurrentTicker}");
            Browser.CoreWebView2?.Navigate($"https://accounts.google.com/ServiceLogin?continue={returnUrl}");
        }

        private void ReturnToFinance_Click(object sender, RoutedEventArgs e)
        {
            LoadTicker(CurrentTicker);
        }

        private void CloseWidget_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}