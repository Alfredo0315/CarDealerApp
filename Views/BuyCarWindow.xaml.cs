using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using CarDealerApp.Models;
using CarDealerApp.Services;

namespace CarDealerApp.Views
{
    public partial class BuyCarWindow : Window
    {
        public string PaymentMethod     { get; private set; } = "";
        public int    PassportSeries    { get; private set; }
        public string PassportNumber    { get; private set; } = "";
        public bool   PassportWasEntered { get; private set; }

        private readonly DatabaseService _dbService;
        private readonly bool _needPassport;

        public BuyCarWindow(Car car, bool needPassport = false)
        {
            InitializeComponent();
            _dbService   = new DatabaseService();
            _needPassport = needPassport;

            txtCarName.Text = $"{car.Mark} {car.Model} ({car.Year_of_release})";
            txtPrice.Text   = $"{car.Price:N0} ₽";
            cmbPayment.SelectedIndex = 0;

            
            if (needPassport)
            {
                passportPanel.Visibility = Visibility.Visible;
                this.Height = 490;
            }
        }

        private void btnConfirm_Click(object sender, RoutedEventArgs e)
        {
           
            if (_needPassport)
            {
                var seriesStr = txtPassportSeries.Text.Trim();
                var numberStr = txtPassportNumber.Text.Trim();

                if (string.IsNullOrEmpty(seriesStr))
                { ShowError("Введите серию паспорта!"); return; }

                if (!Regex.IsMatch(seriesStr, @"^\d{4}$"))
                { ShowError("Серия паспорта — ровно 4 цифры!"); return; }

                if (string.IsNullOrEmpty(numberStr))
                { ShowError("Введите номер паспорта!"); return; }

                if (!Regex.IsMatch(numberStr, @"^\d{6}$"))
                { ShowError("Номер паспорта — ровно 6 цифр!"); return; }

                PassportSeries     = int.Parse(seriesStr);
                PassportNumber     = numberStr;
                PassportWasEntered = true;
            }

            if (cmbPayment.SelectedItem is ComboBoxItem item)
                PaymentMethod = item.Content?.ToString() ?? "Наличные";
            else
                PaymentMethod = "Наличные";

            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void ShowError(string message)
        {
            txtError.Text       = message;
            txtError.Visibility = Visibility.Visible;
        }
    }
}