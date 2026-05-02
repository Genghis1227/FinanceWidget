using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace FinanceWidget
{
    public partial class MainWindow : Window
    {
        public string CurrentTicker { get; private set; }
        public bool UseBetaSite { get; private set; }

        public MainWindow(string ticker, double left = double.NaN, double top = double.NaN, double width = 600, double height = 480, bool keepOnTop = false, bool useBetaSite = true)
        {
            InitializeComponent();
            CurrentTicker = ticker;
            UseBetaSite = useBetaSite;

            if (width > 0) this.Width = width;
            if (height > 0) this.Height = height;

            if (!double.IsNaN(left) && !double.IsNaN(top))
            {
                this.WindowStartupLocation = WindowStartupLocation.Manual;
                this.Left = left;
                this.Top = top;
            }
            
            this.Topmost = keepOnTop;
            KeepOnTopMenuItem.IsChecked = keepOnTop;
            UseBetaSiteMenuItem.IsChecked = useBetaSite;
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
                
                // Set widget background to match site version
                WidgetBorder.Background = UseBetaSite 
                    ? new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#131314"))
                    : System.Windows.Media.Brushes.White;

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
                string script = UseBetaSite ? GetBetaCleanupScript() : GetClassicCleanupScript();
                await Browser.CoreWebView2.ExecuteScriptAsync(script);

                // Show after applying styles to prevent any flash
                await Task.Delay(300);
                BrowserContainer.Opacity = 1;

                // Check if we should suggest login
                CheckLoginStatus();
            }
        }

        private async void CheckLoginStatus()
        {
            if (Browser.CoreWebView2 == null) return;

            bool isLoggedIn = false;
            try
            {
                // Most reliable way to check for Google login is checking for the SID or HSID cookies
                var cookies = await Browser.CoreWebView2.CookieManager.GetCookiesAsync("https://www.google.com");
                isLoggedIn = cookies.Any(c => c.Name == "SID" || c.Name == "HSID");
            }
            catch
            {
                // Fallback to DOM check if cookie manager fails
                string script = @"(function() { return !!document.querySelector('a[href*=""accounts.google.com/ServiceLogin""]') || !!Array.from(document.querySelectorAll('span, div, a, button')).find(el => el.textContent.trim() === 'Sign in'); })()";
                string result = await Browser.CoreWebView2.ExecuteScriptAsync(script);
                isLoggedIn = !(result != null && result.ToLower() == "true");
            }

            if (!isLoggedIn)
            {
                // Show banner
                LoginSuggestionBorder.Visibility = Visibility.Visible;

                // Also show prominent dialog if this is the first check of the session
                if (!App.HasShownLoginPrompt)
                {
                    App.HasShownLoginPrompt = true;
                    var prompt = new LoginPromptWindow();
                    if (prompt.ShowDialog() == true && prompt.WantsToLogin)
                    {
                        LoginToGoogle_Click(this, new RoutedEventArgs());
                    }
                }
            }
            else
            {
                LoginSuggestionBorder.Visibility = Visibility.Collapsed;
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

        private void AddNewWidget_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).SpawnNewWidget();
        }

        private void ReleaseNotes_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            app.OpenUrl("https://github.com/Genghis1227/FinanceWidget/blob/main/ReleaseNotes/release_notes_v" + App.Version + ".md");
        }

        private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            var app = (App)Application.Current;
            await app.CheckForUpdatesAsync(true);
        }

        private void UseBetaSite_Click(object sender, RoutedEventArgs e)
        {
            UseBetaSite = UseBetaSiteMenuItem.IsChecked;
            LoadTicker(CurrentTicker);
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

        private void DismissLoginSuggestion_Click(object sender, RoutedEventArgs e)
        {
            LoginSuggestionBorder.Visibility = Visibility.Collapsed;
        }

        private void CloseWidget_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ResizeThumb_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newWidth = this.Width + e.HorizontalChange;
            double newHeight = this.Height + e.VerticalChange;
            if (newWidth > 150) this.Width = newWidth;
            if (newHeight > 100) this.Height = newHeight;
        }

        private string GetClassicCleanupScript()
        {
            return @"
                var style = document.createElement('style');
                style.textContent = `
                    header, nav, #gb, footer, div.ZhXxed, .rNScrf, .bF2ive, .F1hUFe, .d73ztd, .qN1Zdf, .N35Hn, .tAEEs, .z4ebah, .PF7Y8c, .id-compare-input, .id-compare-button, .id-compare-input-div, .f7t46c, .I6776b { display: none !important; }
                    h1 { font-size: 18px !important; margin: 2px 0 !important; padding: 0 !important; }
                    body { background: white !important; margin: 0 !important; padding: 0 !important; }
                    ::-webkit-scrollbar { display: none !important; }
                `;
                document.head.appendChild(style);

                function isolateChart() {
                    var price = document.querySelector('.YMlS1d') || document.querySelector('.YMlKec.fxKbKc');
                    var chart = document.querySelector('canvas');
                    var timeline = document.querySelector('[role=""tablist""]') || document.querySelector('.D249ge');
                    var tools = document.querySelector('div[role=""toolbar""]') || document.querySelector('.Gxz3Gc-LgbsSe')?.parentElement;

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
                        if (h1 && el.contains(h1)) return;
                        if (timeline && el.contains(timeline)) return;
                        if (tools && el.contains(tools)) return;
                        el.style.setProperty('display', 'none', 'important');
                    }

                    if (price) {
                        var allEls = document.querySelectorAll('body > *, body > * > *');
                        for (var i = 0; i < allEls.length; i++) {
                            var cs = window.getComputedStyle(allEls[i]);
                            if (cs.position === 'fixed' || cs.position === 'sticky') {
                                safeHide(allEls[i]);
                            }
                        }
                    }

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

                    var allBtns = document.querySelectorAll('button, a');
                    for (var i = 0; i < allBtns.length; i++) {
                        var txt = (allBtns[i].textContent || '').trim().toLowerCase();
                        if (txt.includes('compare to')) {
                            var p = allBtns[i];
                            for (var k = 0; k < 4 && p.parentElement && p.parentElement.tagName !== 'BODY'; k++) p = p.parentElement;
                            safeHide(p);
                        }
                    }

                    var inputs = document.querySelectorAll('input[placeholder], textarea[placeholder]');
                    for (var i = 0; i < inputs.length; i++) {
                        var ph = (inputs[i].placeholder || '').toLowerCase();
                        if (ph.includes('search') || ph.includes('ask')) {
                            var p = inputs[i];
                            for (var k = 0; k < 6 && p.parentElement && p.parentElement.tagName !== 'BODY'; k++) p = p.parentElement;
                            safeHide(p);
                        }
                    }

                    if (h1) {
                        var sib = h1.previousElementSibling;
                        while (sib) {
                            sib.style.setProperty('display', 'none', 'important');
                            sib = sib.previousElementSibling;
                        }
                    }

                    if (h1) {
                        var rect = h1.getBoundingClientRect();
                        var scrollTarget = window.pageYOffset + rect.top - 5;
                        document.documentElement.scrollTop = scrollTarget;
                        document.body.scrollTop = scrollTarget;
                    } else {
                        document.documentElement.scrollTop = 65;
                        document.body.scrollTop = 65;
                    }
                }
                isolateChart();
                setInterval(isolateChart, 300);
            ";
        }

        private string GetBetaCleanupScript()
        {
            return @"
                var style = document.createElement('style');
                style.textContent = `
                    /* Hide the top banner and all its variations */
                    header, nav, #gb, footer, [role=""banner""], [role=""navigation""], [role=""complementary""], 
                    [data-wiz-component*=""Header""], [data-wiz-component*=""header""],
                    div.pYTkkf-hSRGPd, .pYTkkf-hSRGPd, div.Njxgkf, div.gguZDb, div.ZhXxed, .XYsxHc, .HwD9Ce,
                    a[aria-label*=""Search""], button[aria-label*=""Settings""], .pYTkkf-Bz112c-LgbsSe,
                    button[aria-label*=""thread""], button[aria-label*=""Research""], button[aria-label*=""history""],
                    /* Hide the sticky header bar, JEPQ/Research tabs bar, and breadcrumb bar */
                    div.OE5XRd, div.Gbn51b, div.PW49pd { 
                        display: none !important; 
                        visibility: hidden !important;
                        height: 0 !important;
                        max-height: 0 !important;
                        overflow: hidden !important;
                    }

                    /* Remove the hardcoded 64px offsets and gaps */
                    body, #yDmH0d, .Y8k45b, .lkrQle, .t4pbz, .XYsxHc, .DJj6Nc, .ZkAH1b, main, .V837kb { 
                        background: transparent !important; 
                        padding-top: 0 !important; 
                        margin-top: 0 !important; 
                        top: 0 !important;
                    }

                    body, #yDmH0d {
                        margin: 0 !important; 
                        padding: 0 !important; 
                        overflow: hidden !important; 
                    }

                    /* Give a small top padding so the title is visible */
                    .Y8k45b, .lkrQle, main {
                        padding-top: 8px !important;
                    }

                    ::-webkit-scrollbar { display: none !important; }
                `;
                document.head.appendChild(style);

                function isolateChart() {
                    var price = document.querySelector('.YMlS1d') || document.querySelector('.YMlKec.fxKbKc') || document.querySelector('div.V837kb');
                    var chart = document.querySelector('canvas');
                    var timeline = document.querySelector('[role=""tablist""]') || document.querySelector('.D249ge');
                    var breadcrumb = document.querySelector('a.DJj6Nc') || document.querySelector('.DJj6Nc');

                    // Robust Title Detection
                    var title = null;
                    var allH1s = document.querySelectorAll('h1');
                    for (var qi = 0; qi < allH1s.length; qi++) {
                        var t = (allH1s[qi].textContent || '').trim();
                        if (t.length > 5 && t !== 'Finance') { title = allH1s[qi]; break; }
                    }

                    if (!title && price) {
                        // Look for a large sibling of the price container
                        var parent = price.closest('.Y8k45b') || price.parentElement;
                        if (parent) {
                            var candidates = parent.querySelectorAll('div, span');
                            for (var i = 0; i < candidates.length; i++) {
                                var txt = (candidates[i].textContent || '').trim();
                                // Title is usually long and not the price itself
                                if (txt.length > 10 && !candidates[i].contains(price) && !candidates[i].querySelector('canvas')) {
                                    title = candidates[i];
                                    break;
                                }
                            }
                        }
                    }

                    function safeHide(el) {
                        if (!el || el.tagName === 'BODY' || el.tagName === 'HTML') return;
                        if (price && el.contains(price)) return;
                        if (chart && el.contains(chart)) return;
                        if (title && el.contains(title)) return;
                        if (timeline && el.contains(timeline)) return;
                        if (breadcrumb && el.contains(breadcrumb)) return;
                        
                        // Don't hide the main container siblings if they might contain the title
                        if (el.classList.contains('Y8k45b') || el.classList.contains('lkrQle')) return;

                        el.style.setProperty('display', 'none', 'important');
                    }

                    if (price) {
                        var allEls = document.querySelectorAll('body > *, body > * > *');
                        for (var i = 0; i < allEls.length; i++) {
                            // Specifically hide the top bar
                            if (allEls[i].classList.contains('pYTkkf-hSRGPd')) {
                                allEls[i].style.setProperty('display', 'none', 'important');
                                continue;
                            }
                            var cs = window.getComputedStyle(allEls[i]);
                            if (cs.position === 'fixed' || cs.position === 'sticky') {
                                safeHide(allEls[i]);
                            }
                        }
                    }

                    var badTexts = ['research', 'search for stocks', 'search or ask a question', 'build a watchlist', 'you may be interested in', 'discover more', 'compare markets', 'overview', 'financials', 'about'];
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

                    // Hide everything below the timeline tabs (Overview, Financials, etc.)
                    if (timeline) {
                        var timelineContainer = timeline.closest('[class]') || timeline.parentElement;
                        if (timelineContainer) {
                            var nextSib = timelineContainer.nextElementSibling;
                            while (nextSib) {
                                safeHide(nextSib);
                                nextSib = nextSib.nextElementSibling;
                            }
                        }
                    }

                    // Also hide tab-like elements below the chart (Overview, Financials, etc.)
                    var allLinks = document.querySelectorAll('a, button, span');
                    var tabTexts = ['overview', 'financials', 'about'];
                    for (var i = 0; i < allLinks.length; i++) {
                        var linkTxt = (allLinks[i].textContent || '').trim().toLowerCase();
                        for (var t = 0; t < tabTexts.length; t++) {
                            if (linkTxt === tabTexts[t]) {
                                var tabContainer = allLinks[i].parentElement;
                                if (tabContainer && tabContainer !== timeline) {
                                    safeHide(tabContainer);
                                }
                                break;
                            }
                        }
                    }

                    var inputs = document.querySelectorAll('input[placeholder], textarea[placeholder]');
                    for (var i = 0; i < inputs.length; i++) {
                        var ph = (inputs[i].placeholder || '').toLowerCase();
                        if (ph.includes('search') || ph.includes('ask')) {
                            var p = inputs[i];
                            for (var k = 0; k < 6 && p.parentElement && p.parentElement.tagName !== 'BODY'; k++) p = p.parentElement;
                            safeHide(p);
                        }
                    }

                    if (title) {
                        var sib = title.previousElementSibling;
                        while (sib) {
                            if (breadcrumb && sib.contains(breadcrumb)) break;
                            sib.style.setProperty('display', 'none', 'important');
                            sib = sib.previousElementSibling;
                        }
                    }

                    // Small negative scroll to push content down slightly for title visibility
                    document.documentElement.scrollTop = 0;
                    document.body.scrollTop = 0;
                }
                isolateChart();
                setInterval(isolateChart, 300);
            ";
        }
    }
}
