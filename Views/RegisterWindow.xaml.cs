using System.Windows;
using CarDealerApp.Services;

namespace CarDealerApp.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly DatabaseService _dbService;

        public RegisterWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var login           = txtLogin.Text.Trim();
            var password        = txtPassword.Password;
            var confirmPassword = txtConfirmPassword.Password;

            // Роль всегда Client — пользователь не может её выбрать
            const string role = "Client";

            if (string.IsNullOrEmpty(login))
            {
                ShowError("Введите логин!");
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                ShowError("Введите пароль!");
                return;
            }

            if (password != confirmPassword)
            {
                ShowError("Пароли не совпадают!");
                return;
            }

            if (_dbService.RegisterUser(login, password, role, out var error))
            {
                MessageBox.Show(
                    $"Аккаунт «{login}» успешно создан!\nВы зарегистрированы как Клиент.",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                ShowError(error);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}