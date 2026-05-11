using System.Windows;
using CarDealerApp.Models;
using CarDealerApp.Services;

namespace CarDealerApp.Views
{
    //авторизации пользователя
    public partial class LoginWindow : Window
    {
        private readonly DatabaseService _dbService;

        public LoginWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }
        
        //обработка нажатия кнопки "Войти"
        private void btnLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = txtLogin.Text.Trim();
            var password = txtPassword.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Введите логин и пароль!");
                return;
            }

            if (_dbService.Authenticate(login, password, out var role))
            {
                CurrentUser.Login = login;
                CurrentUser.Role = role;

                // открытие главного меню
                var mainWindow = new MainWindow();
                mainWindow.Show();
                this.Close();
            }
            else
            {
                ShowError("Неверный логин или пароль!");
            }
        }
        
        //открытие окна регистрации
        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var regWindow = new RegisterWindow();
            regWindow.ShowDialog();
        }

        private void ShowError(string message)
        {
            txtError.Text = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}