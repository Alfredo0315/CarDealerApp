using System.Windows;
using CarDealerApp.Models;

namespace CarDealerApp.Views
{
    // добавления/редактирования дилера
    public partial class DillerEditWindow : Window
    {
        public Diller Diller { get; private set; }

        public DillerEditWindow(Diller? diller = null)
        {
            InitializeComponent();

            if (diller != null)
            {
                Diller = diller;
                txtTitle.Text = "Редактирование дилера";
                // заполнение
                txtName.Text  = diller.Car_center_name;
                txtPhone.Text = diller.Phone_number;
                txtEmail.Text = diller.Email;
            }
            else
            {
                Diller = new Diller();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // валидация
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Введите название центра!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // сохранение
            Diller.Car_center_name = txtName.Text.Trim();
            Diller.Phone_number    = txtPhone.Text.Trim();
            Diller.Email           = txtEmail.Text.Trim();

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