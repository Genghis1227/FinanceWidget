using System.Windows;

namespace FinanceWidget
{
    public partial class LoginPromptWindow : Window
    {
        public bool WantsToLogin { get; private set; } = false;

        public LoginPromptWindow()
        {
            InitializeComponent();
        }

        private void SignIn_Click(object sender, RoutedEventArgs e)
        {
            WantsToLogin = true;
            this.DialogResult = true;
            this.Close();
        }

        private void Later_Click(object sender, RoutedEventArgs e)
        {
            WantsToLogin = false;
            this.DialogResult = false;
            this.Close();
        }
    }
}
