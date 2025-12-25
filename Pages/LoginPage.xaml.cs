using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Pract15.Services;

namespace Pract15.Pages
{
    public partial class LoginPage : Page
    {
        public LoginPage()
        {
            InitializeComponent();
            PinPasswordBox.Focus();
            PinPasswordBox.KeyDown += PinPasswordBox_KeyDown;
        }

        private void ManagerLogin_Click(object sender, RoutedEventArgs e)
        {
            string pin = PinPasswordBox.Password;

            if (string.IsNullOrEmpty(pin))
            {
                ShowError("Введите пин-код");
                return;
            }

            if (AuthService.LoginAsManager(pin))
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                mainWindow?.MainFrame.Navigate(new MainPage());
            }
            else
            {
                ShowError("Неверный пин-код");
            }
        }

        private void VisitorLogin_Click(object sender, RoutedEventArgs e)
        {
            AuthService.LoginAsVisitor();
            var mainWindow = Application.Current.MainWindow as MainWindow;
            mainWindow?.MainFrame.Navigate(new MainPage());
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void PinPasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ManagerLogin_Click(sender, e);
            }
        }
    }
}