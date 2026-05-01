using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FinanceWidget
{
    public partial class AddTickerWindow : Window
    {
        public string Ticker { get; private set; } = string.Empty;

        public AddTickerWindow()
        {
            InitializeComponent();
            TickerInput.Focus();
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                this.DragMove();
            }
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            Submit();
        }

        private void Example_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string ticker)
            {
                TickerInput.Text = ticker;
                TickerInput.Focus();
                TickerInput.SelectionStart = TickerInput.Text.Length;
            }
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var lookup = new LookupWindow();
            lookup.Show();
            this.Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void TickerInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Submit();
            }
            else if (e.Key == Key.Escape)
            {
                DialogResult = false;
                Close();
            }
        }

        private void Submit()
        {
            Ticker = TickerInput.Text.Trim().ToUpper();
            if (!string.IsNullOrEmpty(Ticker))
            {
                DialogResult = true;
                Close();
            }
        }
    }
}
