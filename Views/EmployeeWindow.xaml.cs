using System;
using System.Windows;
using CarDealerApp.Models;

namespace CarDealerApp.Views
{
    //редактирования/добавления сотрудника
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

                // заполнение
                txtName.Text = employee.Name;
                txtSurname.Text = employee.Surname;
                txtMiddleName.Text = employee.Middle_name ?? "";
                txtPost.Text = employee.Post;
                txtBusinessPhone.Text = employee.Business_phone_number;
            }
            else
            {
                Employee = new Employee();
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            // валидация
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

            if (string.IsNullOrWhiteSpace(txtPost.Text))
            {
                MessageBox.Show("Введите должность!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtBusinessPhone.Text))
            {
                MessageBox.Show("Введите рабочий телефон!", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // сохранение
            Employee.Name = txtName.Text.Trim();
            Employee.Surname = txtSurname.Text.Trim();
            Employee.Middle_name = string.IsNullOrWhiteSpace(txtMiddleName.Text) ? null : txtMiddleName.Text.Trim();
            Employee.Post = txtPost.Text.Trim();
            Employee.Business_phone_number = txtBusinessPhone.Text.Trim();

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