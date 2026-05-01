using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

namespace FinanceWidget
{
    public partial class LookupWindow : Window
    {
        public class TickerInfo
        {
            public string Symbol { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
        }

        public LookupWindow()
        {
            InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            StocksList.ItemsSource = new List<TickerInfo>
            {
                new TickerInfo { Symbol = "AAPL:NASDAQ", Name = "Apple Inc." },
                new TickerInfo { Symbol = "MSFT:NASDAQ", Name = "Microsoft Corporation" },
                new TickerInfo { Symbol = "TSLA:NASDAQ", Name = "Tesla, Inc." },
                new TickerInfo { Symbol = "NVDA:NASDAQ", Name = "NVIDIA Corporation" },
                new TickerInfo { Symbol = "GOOGL:NASDAQ", Name = "Alphabet Inc." },
                new TickerInfo { Symbol = "AMZN:NASDAQ", Name = "Amazon.com, Inc." },
                new TickerInfo { Symbol = "META:NASDAQ", Name = "Meta Platforms, Inc." },
                new TickerInfo { Symbol = "NFLX:NASDAQ", Name = "Netflix, Inc." }
            };

            EtfsList.ItemsSource = new List<TickerInfo>
            {
                new TickerInfo { Symbol = "SPY:NYSEARCA", Name = "SPDR S&P 500 ETF Trust" },
                new TickerInfo { Symbol = "QQQ:NASDAQ", Name = "Invesco QQQ Trust" },
                new TickerInfo { Symbol = "VOO:NYSEARCA", Name = "Vanguard S&P 500 ETF" },
                new TickerInfo { Symbol = "JEPQ:NASDAQ", Name = "JPMorgan Nasdaq Equity Premium Income ETF" },
                new TickerInfo { Symbol = "SCHD:NYSEARCA", Name = "Schwab US Dividend Equity ETF" },
                new TickerInfo { Symbol = "VTI:NYSEARCA", Name = "Vanguard Total Stock Market ETF" },
                new TickerInfo { Symbol = "ARKK:NYSEARCA", Name = "ARK Innovation ETF" }
            };

            CryptoList.ItemsSource = new List<TickerInfo>
            {
                new TickerInfo { Symbol = "BTC-USD", Name = "Bitcoin" },
                new TickerInfo { Symbol = "ETH-USD", Name = "Ethereum" },
                new TickerInfo { Symbol = "SOL-USD", Name = "Solana" },
                new TickerInfo { Symbol = "DOGE-USD", Name = "Dogecoin" },
                new TickerInfo { Symbol = "ADA-USD", Name = "Cardano" },
                new TickerInfo { Symbol = "XRP-USD", Name = "XRP" }
            };
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void AddTicker_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is string ticker)
            {
                ((App)Application.Current).SpawnNewWidgetFromTicker(ticker);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
