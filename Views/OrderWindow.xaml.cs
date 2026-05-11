using System;
using System.Windows;
using CarDealerApp.Models;

namespace CarDealerApp.Views
{
    //редактирования/добавления заказа
    public partial class OrderWindow : Window
    {
        public Order Order { get; private set; }
        public int CarId { get; private set; }
        private readonly bool _isEdit;

        public OrderWindow(Order? order = null, int carId = 0)
        {
            InitializeComponent();

            if (order != null)
            {
                _isEdit = true;
                Order = order;
                txtTitle.Text = "Редактирование заказа";

                // заполнение полей
                txtClientId.Text = order.ID_Client.ToString();
                txtOrderStatus.Text = order.Order_status;
                dpDateOfExecution.SelectedDate = order.Date_of_execution;
                txtPaymentMethod.Text = order.Payment_method;
                txtCarId.Text = carId > 0 ? carId.ToString() : "";
                txtCarId.IsEnabled = false;
            }
            else
            {
                Order = new Order();
                dpDateOfExecution.SelectedDate = DateTime.Now;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // валидация
            if (!int.TryParse(txtClientId.Text, out int clientId) || clientId <= 0)
            {
                MessageBox.Show("Введите корректный ID клиента!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtCarId.Text, out int carId) || carId <= 0)
            {
                MessageBox.Show("Введите корректный ID автомобиля!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtOrderStatus.Text))
            {
                MessageBox.Show("Введите статус заказа!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (dpDateOfExecution.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату исполнения!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtPaymentMethod.Text))
            {
                MessageBox.Show("Введите способ оплаты!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // сохраняем
            Order.ID_Client = clientId;
            Order.Order_status = txtOrderStatus.Text.Trim();
            Order.Date_of_execution = dpDateOfExecution.SelectedDate.Value;
            Order.Payment_method = txtPaymentMethod.Text.Trim();
            CarId = carId;

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