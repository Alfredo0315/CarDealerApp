using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using CarDealerApp.Models;
using CarDealerApp.Services;

namespace CarDealerApp.Views
{
    public partial class OrderWindow : Window
    {
        public Order Order   { get; private set; }
        public int   CarId   { get; private set; }
        private readonly bool _isEdit;
        private readonly DatabaseService _dbService;

       
        public OrderWindow(List<Car> cars,
                           List<(int Id, string Phone, string Name)> clients)
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            Order  = new Order();
            _isEdit = false;

            dpDateOfExecution.SelectedDate = DateTime.Now;
            cmbOrderStatus.SelectedIndex   = 0;
            cmbPaymentMethod.SelectedIndex = 0;

           
            cmbClient.Visibility     = Visibility.Visible;
            txtClientPhone.Visibility = Visibility.Collapsed;
            foreach (var (id, phone, name) in clients)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{phone}  ({name})",
                    Tag     = id
                };
                cmbClient.Items.Add(item);
            }
            if (cmbClient.Items.Count > 0)
                cmbClient.SelectedIndex = 0;

           
            cmbCar.Visibility     = Visibility.Visible;
            txtCarName.Visibility = Visibility.Collapsed;
            foreach (var car in cars)
            {
                var item = new ComboBoxItem
                {
                    Content = $"{car.Mark} {car.Model} ({car.Year_of_release})",
                    Tag     = car.ID_Car
                };
                cmbCar.Items.Add(item);
            }
            if (cmbCar.Items.Count > 0)
                cmbCar.SelectedIndex = 0;
        }

       
        public OrderWindow(Order order, string carName, string clientPhone)
        {
            InitializeComponent();
            _dbService = new DatabaseService();
            _isEdit = true;
            Order = order;
            txtTitle.Text = "Редактирование заказа";

            
            cmbClient.Visibility      = Visibility.Collapsed;
            txtClientPhone.Visibility = Visibility.Visible;
            txtClientPhone.Text       = clientPhone;

            
            cmbCar.Visibility     = Visibility.Collapsed;
            txtCarName.Visibility = Visibility.Visible;
            txtCarName.Text       = string.IsNullOrEmpty(carName) ? "—" : carName;

            SelectComboItem(cmbOrderStatus,   order.Order_status);

         
            dpDateOfExecution.Visibility = Visibility.Collapsed;
            txtDateReadonly.Visibility   = Visibility.Visible;
            txtDateReadonly.Text         = order.Date_of_execution.ToString("dd.MM.yyyy");

            SelectComboItem(cmbPaymentMethod, order.Payment_method);
        }

        private static void SelectComboItem(ComboBox combo, string? text)
        {
            if (string.IsNullOrEmpty(text)) return;
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content?.ToString()?.StartsWith(text) == true ||
                    item.Content?.ToString() == text)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            if (combo.Items.Count > 0)
                combo.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEdit)
            {
               
                if (cmbClient.SelectedItem is not ComboBoxItem clientItem ||
                    clientItem.Tag is not int selectedClientId)
                {
                    MessageBox.Show("Выберите клиента!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Order.ID_Client = selectedClientId;

               
                if (cmbCar.SelectedItem is not ComboBoxItem carItem ||
                    carItem.Tag is not int selectedCarId)
                {
                    MessageBox.Show("Выберите автомобиль!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                CarId = selectedCarId;
            }
      

            if (cmbOrderStatus.SelectedItem is not ComboBoxItem statusItem)
            {
                MessageBox.Show("Выберите статус заказа!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

         
            if (!_isEdit)
            {
                if (dpDateOfExecution.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату исполнения!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
           
                if (dpDateOfExecution.SelectedDate.Value.Date > DateTime.Today)
                {
                    MessageBox.Show("Дата исполнения не может быть позже сегодняшней!", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                Order.Date_of_execution = dpDateOfExecution.SelectedDate.Value;
            }
           
            if (cmbPaymentMethod.SelectedItem is not ComboBoxItem paymentItem)
            {
                MessageBox.Show("Выберите способ оплаты!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Order.Order_status      = statusItem.Content?.ToString()  ?? "В обработке";
          
            Order.Payment_method    = paymentItem.Content?.ToString() ?? "Наличные";

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