using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CarDealerApp.Models;
using Microsoft.Win32;

namespace CarDealerApp.Views
{
    public partial class CarEditWindow : Window
    {
        public Car Car { get; private set; }
        private readonly bool _isEdit;

      
        private string? _selectedPhotoPath;

        public CarEditWindow(Car? car = null)
        {
            InitializeComponent();

            
            var role = CurrentUser.Role;
            if (role == UserRole.Admin || role == UserRole.Employee)
                photoPanel.Visibility = Visibility.Visible;

            if (car != null)
            {
                _isEdit = true;
                Car = car;
                txtTitle.Text = "Редактирование автомобиля";

                txtMark.Text  = car.Mark;
                txtModel.Text = car.Model;
                txtColor.Text = car.Color;
                txtYear.Text  = car.Year_of_release.ToString();
                txtPrice.Text = car.Price.ToString();
                txtTech.Text  = car.Technical_specifications;

               
                if (!string.IsNullOrEmpty(car.PhotoPath) && File.Exists(car.PhotoPath))
                {
                    _selectedPhotoPath = car.PhotoPath;
                    ShowPhoto(car.PhotoPath);
                }
            }
            else
            {
                Car = new Car();
            }
        }

        
        private void btnChoosePhoto_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title  = "Выберите фото автомобиля",
                Filter = "Изображения (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                _selectedPhotoPath = dialog.FileName;
                ShowPhoto(_selectedPhotoPath);
            }
        }

       
        private void btnClearPhoto_Click(object sender, RoutedEventArgs e)
        {
            _selectedPhotoPath = null;
            imgPreview.Source       = null;
            imgPreview.Visibility   = Visibility.Collapsed;
            photoPlaceholder.Visibility = Visibility.Visible;
            btnClearPhoto.Visibility    = Visibility.Collapsed;
        }

        
        private void ShowPhoto(string path)
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource        = new Uri(path, UriKind.Absolute);
                bitmap.CacheOption      = BitmapCacheOption.OnLoad;
                
                bitmap.DecodePixelWidth = 600;
                bitmap.EndInit();

                imgPreview.Source           = bitmap;
                imgPreview.Visibility       = Visibility.Visible;
                photoPlaceholder.Visibility = Visibility.Collapsed;
                btnClearPhoto.Visibility    = Visibility.Visible;
            }
            catch
            {
                
                imgPreview.Visibility       = Visibility.Collapsed;
                photoPlaceholder.Visibility = Visibility.Visible;
                btnClearPhoto.Visibility    = Visibility.Collapsed;
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
           
            if (string.IsNullOrWhiteSpace(txtMark.Text))
            {
                MessageBox.Show("Введите марку!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(txtYear.Text, out int year) || year < 1900 || year > DateTime.Now.Year)
            {
                MessageBox.Show("Введите корректный год выпуска!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


         
            Car.Mark                     = txtMark.Text.Trim();
            Car.Model                    = txtModel.Text.Trim();
            Car.Color                    = txtColor.Text.Trim();
            Car.Year_of_release          = year;
            Car.Price                    = price;
            Car.Technical_specifications = txtTech.Text.Trim();
            Car.PhotoPath                = _selectedPhotoPath;

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