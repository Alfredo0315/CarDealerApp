using System.Windows;
using CarDealerApp.Models;
using CarDealerApp.Services;

namespace CarDealerApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly DatabaseService _dbService;
        private bool _showPassword = false;

        public LoginWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            _showPassword = !_showPassword;

            if (_showPassword)
            {
   
                txtPasswordVisible.Text = txtPassword.Password;
                txtPassword.Visibility        = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                eyeIcon.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(26, 58, 40));
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
 
                txtPassword.Password          = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility        = Visibility.Visible;
                eyeIcon.Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(138, 154, 187));
                txtPassword.Focus();
            }
        }

        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var email = txtLogin.Text.Trim();
    
            var password = _showPassword ? txtPasswordVisible.Text : txtPassword.Password;

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите email и пароль!");
                return;
            }

            var result = _dbService.Authenticate(email, password);

            if (result == null)
            {
                ShowError("Неверный email или пароль!");
                return;
            }

            CurrentUser.Email      = email;
            CurrentUser.Role       = result.Role;
            CurrentUser.ClientId   = result.ClientId;
            CurrentUser.EmployeeId = result.EmployeeId;

            var mainWindow = new MainWindow();
            mainWindow.Show();
            this.Close();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var regWindow = new RegisterWindow();
            regWindow.ShowDialog();
        }

        private void ShowError(string message)
        {
            txtError.Text       = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}