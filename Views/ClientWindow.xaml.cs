using System;
using System.Windows;
using CarDealerApp.Models;

namespace CarDealerApp.Views
{
    
    public partial class ClientWindow : Window
    {
        public Client Client { get; private set; }
        private readonly bool _isEdit;

        public ClientWindow(Client? client = null)
        {
            InitializeComponent();

            if (client != null)
            {
                _isEdit = true;
                Client = client;
                txtTitle.Text = "Редактирование клиента";

                txtName.Text = client.Name;
                txtSurname.Text = client.Surname;
                txtMiddleName.Text       = client.Middle_name ?? "";
                txtMiddleName.IsReadOnly = true;   // отчество нельзя менять
                txtMiddleName.Background = System.Windows.Media.Brushes.LightGray;
                txtPassportSeries.Text = client.Passport_series.ToString();
                txtPassportNumber.Text = client.Passport_number;
                txtPhone.Text = client.Phone_number;
                txtEmail.Text = client.Email;
            }
            else
            {
                Client = new Client();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите имя!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtSurname.Text))
            {
                MessageBox.Show("Введите фамилию!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtPassportSeries.Text, out int passportSeries) || passportSeries <= 0)
            {
                MessageBox.Show("Введите корректную серию паспорта!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPassportNumber.Text))
            {
                MessageBox.Show("Введите номер паспорта!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPhone.Text))
            {
                MessageBox.Show("Введите телефон!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            
            Client.Name = txtName.Text.Trim();
            Client.Surname = txtSurname.Text.Trim();
            Client.Middle_name = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
            Client.Passport_series = passportSeries;
            Client.Passport_number = txtPassportNumber.Text.Trim();
            Client.Phone_number = txtPhone.Text.Trim();
            Client.Email = txtEmail.Text.Trim();

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}