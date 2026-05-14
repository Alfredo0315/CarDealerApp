using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CarDealerApp.Models;

namespace CarDealerApp.Views
{
    public partial class EmployeeWindow : Window
    {
        public Employee Employee { get; private set; }
        private readonly bool _isEdit;

        public EmployeeWindow(Employee? employee = null)
        {
            InitializeComponent();

            if (employee != null)
            {
                _isEdit = true;
                Employee = employee;
                txtTitle.Text = "Редактирование сотрудника";

                txtName.Text          = employee.Name;
                txtSurname.Text       = employee.Surname;
                txtMiddleName.Text    = employee.Middle_name ?? "";
                txtMiddleName.IsReadOnly = true;  // отчество нельзя менять
                txtMiddleName.Background = System.Windows.Media.Brushes.LightGray;
                txtBusinessPhone.Text = employee.Business_phone_number;
                txtEmail.Text         = employee.Email ?? "";

              
                SelectComboItem(cmbPost, employee.Post);
            }
            else
            {
                Employee = new Employee();
                cmbPost.SelectedIndex = 0;
            }
        }

     
        private static void SelectComboItem(ComboBox combo, string? text)
        {
            if (string.IsNullOrEmpty(text)) return;
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Content?.ToString() == text)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            combo.SelectedIndex = 0;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            { MessageBox.Show("Введите имя!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (string.IsNullOrWhiteSpace(txtSurname.Text))
            { MessageBox.Show("Введите фамилию!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (cmbPost.SelectedItem is not ComboBoxItem postItem)
            { MessageBox.Show("Выберите должность!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            
            var phone = txtBusinessPhone.Text.Trim();
            if (string.IsNullOrWhiteSpace(phone))
            { MessageBox.Show("Введите рабочий телефон!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            if (!Regex.IsMatch(phone, @"^8\d{10}$"))
            { MessageBox.Show("Телефон должен содержать 11 цифр и начинаться с 8.\nПример: 84951234567", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

           
            var email = txtEmail.Text.Trim();
            if (!string.IsNullOrEmpty(email) &&
                !Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            { MessageBox.Show("Введите корректный email!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning); return; }

            Employee.Name                  = txtName.Text.Trim();
            Employee.Surname               = txtSurname.Text.Trim();
            Employee.Middle_name           = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
            Employee.Post                  = postItem.Content?.ToString() ?? "";
            Employee.Business_phone_number = phone;
            Employee.Email                 = string.IsNullOrEmpty(email) ? null : email;

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