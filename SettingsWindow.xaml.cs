using System.Windows;

namespace FinanceWidget
{
    public partial class SettingsWindow : Window
    {
        public string Ticker { get; private set; }
        public bool UseBetaSite { get; private set; }

        public SettingsWindow(string currentTicker, bool useBetaSite = false)
        {
            InitializeComponent();
            Ticker = currentTicker;
            UseBetaSite = useBetaSite;

            TickerTextBox.Text = currentTicker;
            UseBetaCheckBox.IsChecked = useBetaSite;
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Ticker = TickerTextBox.Text.Trim();
            if (string.IsNullOrEmpty(Ticker))
            {
                // If empty, just don't change anything (or could show a message)
                // For now, let's just close without error if they didn't mean to change it
                DialogResult = false; 
                Close();
                return;
            }
            UseBetaSite = UseBetaCheckBox.IsChecked ?? false;
            DialogResult = true;
            Close();
        }
    }
}
