using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Media;
using CarDealerApp.Services;

namespace CarDealerApp.Views
{
    public partial class RegisterWindow : Window
    {
        private readonly DatabaseService _dbService;
        private bool _showPassword        = false;
        private bool _showConfirmPassword = false;

        public RegisterWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();
        }

        private void btnShowPassword_Click(object sender, RoutedEventArgs e)
        {
            _showPassword = !_showPassword;

            if (_showPassword)
            {
                txtPasswordVisible.Text       = txtPassword.Password;
                txtPassword.Visibility        = Visibility.Collapsed;
                txtPasswordVisible.Visibility = Visibility.Visible;
                eyeIcon1.Foreground = new SolidColorBrush(Color.FromRgb(26, 58, 40));
                txtPasswordVisible.Focus();
                txtPasswordVisible.CaretIndex = txtPasswordVisible.Text.Length;
            }
            else
            {
                txtPassword.Password          = txtPasswordVisible.Text;
                txtPasswordVisible.Visibility = Visibility.Collapsed;
                txtPassword.Visibility        = Visibility.Visible;
                eyeIcon1.Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 187));
                txtPassword.Focus();
            }
        }

        private void btnShowConfirmPassword_Click(object sender, RoutedEventArgs e)
        {
            _showConfirmPassword = !_showConfirmPassword;

            if (_showConfirmPassword)
            {
                txtConfirmPasswordVisible.Text       = txtConfirmPassword.Password;
                txtConfirmPassword.Visibility        = Visibility.Collapsed;
                txtConfirmPasswordVisible.Visibility = Visibility.Visible;
                eyeIcon2.Foreground = new SolidColorBrush(Color.FromRgb(26, 58, 40));
                txtConfirmPasswordVisible.Focus();
                txtConfirmPasswordVisible.CaretIndex = txtConfirmPasswordVisible.Text.Length;
            }
            else
            {
                txtConfirmPassword.Password          = txtConfirmPasswordVisible.Text;
                txtConfirmPasswordVisible.Visibility = Visibility.Collapsed;
                txtConfirmPassword.Visibility        = Visibility.Visible;
                eyeIcon2.Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 187));
                txtConfirmPassword.Focus();
            }
        }

        private void btnRegister_Click(object sender, RoutedEventArgs e)
        {
            var surname    = txtSurname.Text.Trim();
            var name       = txtName.Text.Trim();
            var middleName = txtMiddleName.Text.Trim();
            var phone      = txtPhone.Text.Trim();
            var email      = txtEmail.Text.Trim();
            
            var password = _showPassword        ? txtPasswordVisible.Text        : txtPassword.Password;
            var confirm  = _showConfirmPassword ? txtConfirmPasswordVisible.Text : txtConfirmPassword.Password;

            if (string.IsNullOrEmpty(surname))
            { ShowError("Введите фамилию!"); return; }

            if (string.IsNullOrEmpty(name))
            { ShowError("Введите имя!"); return; }

            if (string.IsNullOrEmpty(phone))
            { ShowError("Введите номер телефона!"); return; }

            if (!Regex.IsMatch(phone, @"^8\d{10}$"))
            { ShowError("Телефон должен содержать 11 цифр и начинаться с 8.\nПример: 89991234567"); return; }

            if (string.IsNullOrEmpty(email))
            { ShowError("Введите email!"); return; }

            if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { ShowError("Введите корректный email!"); return; }

            if (string.IsNullOrEmpty(password))
            { ShowError("Введите пароль!"); return; }

            if (!DatabaseService.ValidatePassword(password, out var pwdError))
            { ShowError(pwdError); return; }

            if (password != confirm)
            { ShowError("Пароли не совпадают!"); return; }

            var result = _dbService.RegisterClient(
                name:       name,
                surname:    surname,
                middleName: string.IsNullOrEmpty(middleName) ? null : middleName,
                phone:      phone,
                email:      email,
                password:   password,
                error:      out var regError);

            if (result)
            {
                MessageBox.Show(
                    $"Аккаунт успешно создан!\nВойдите с email: {email}",
                    "Успех",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                this.Close();
            }
            else
            {
                ShowError(regError);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void ShowError(string message)
        {
            txtError.Text       = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}