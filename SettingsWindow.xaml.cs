using System.Windows;

namespace FinanceWidget
{
    public partial class SettingsWindow : Window
    {
        public string Ticker { get; private set; }

        public SettingsWindow(string currentTicker)
        {
            InitializeComponent();
            Ticker = currentTicker;

            TickerTextBox.Text = currentTicker;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Ticker = TickerTextBox.Text.Trim();
            if (string.IsNullOrEmpty(Ticker))
            {
                Ticker = "JEPQ:NASDAQ";
            }
            DialogResult = true;
            Close();
        }
    }
}
