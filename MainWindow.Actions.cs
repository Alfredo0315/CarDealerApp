using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using CarDealerApp.Models;
using CarDealerApp.Views;

namespace CarDealerApp
{
    public partial class MainWindow : Window
    {
        

        private void btnCars_Click(object sender, RoutedEventArgs e)      => LoadCars();
        private void btnClients_Click(object sender, RoutedEventArgs e)   => LoadClients();
        private void btnOrders_Click(object sender, RoutedEventArgs e)    => LoadOrders();
        private void btnEmployees_Click(object sender, RoutedEventArgs e) => LoadEmployees();
        private void btnDillers_Click(object sender, RoutedEventArgs e)   => LoadDillers();

        private void btnReports_Click(object sender, RoutedEventArgs e)
        {
            if (CurrentUser.Role != UserRole.Admin) return;
            var result = MessageBox.Show(
                "Нажмите «Да» для экспорта автомобилей, «Нет» для экспорта заказов.",
                "Выберите отчёт",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
                ExportToExcel(ViewMode.Cars);
            else if (result == MessageBoxResult.No)
                ExportToExcel(ViewMode.Orders);
        }

        private void btnLogout_Click(object sender, RoutedEventArgs e)
        {
            CurrentUser.Login = null;
            CurrentUser.Role  = null;
            var loginWindow = new Views.LoginWindow();
            loginWindow.Show();
            this.Close();
        }

       

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEdit()) return;
            try
            {
                switch (_currentView)
                {
                    case ViewMode.Cars:      AddCar();      break;
                    case ViewMode.Clients:   AddClient();   break;
                    case ViewMode.Orders:    AddOrder();    break;
                    case ViewMode.Employees: AddEmployee(); break;
                    case ViewMode.Dillers:   AddDiller();   break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddCar()
        {
            var dlg = new Views.CarEditWindow();
            if (dlg.ShowDialog() == true)
            {
                _dbService.AddCar(dlg.Car);
                LoadCars();
            }
        }

        private void AddClient()
        {
            var dlg = new Views.ClientWindow();
            if (dlg.ShowDialog() == true)
            {
                _dbService.AddClient(dlg.Client);
                LoadClients();
            }
        }

        private void AddOrder()
        {
            var cars    = _dbService.GetAllCars();
            var clients = _dbService.GetClientPhones();
            var dlg     = new Views.OrderWindow(cars, clients);
            if (dlg.ShowDialog() == true)
            {
                _dbService.AddOrder(dlg.Order, dlg.CarId);
                LoadOrders();
            }
        }

        private void AddEmployee()
        {
            var dlg = new Views.EmployeeWindow();
            if (dlg.ShowDialog() == true)
            {
                _dbService.AddEmployee(dlg.Employee);
                LoadEmployees();
            }
        }

        private void AddDiller()
        {
            var dlg = new Views.DillerEditWindow();
            if (dlg.ShowDialog() == true)
            {
                _dbService.AddDiller(dlg.Diller);
                LoadDillers();
            }
        }

      

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEdit()) return;
            if (GetSelectedItem() == null)
            {
                MessageBox.Show("Выберите запись для редактирования!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            try
            {
                switch (_currentView)
                {
                    case ViewMode.Cars:      EditCar();      break;
                    case ViewMode.Clients:   EditClient();   break;
                    case ViewMode.Orders:    EditOrder();    break;
                    case ViewMode.Employees: EditEmployee(); break;
                    case ViewMode.Dillers:   EditDiller();   break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditCar()
        {
            var car = (Car)GetSelectedItem()!;
            var dlg = new Views.CarEditWindow(car);
            if (dlg.ShowDialog() == true)
            {
                _dbService.UpdateCar(dlg.Car);
                LoadCars();
            }
        }

        private void EditClient()
        {
            var client = (Client)GetSelectedItem()!;
            var dlg    = new Views.ClientWindow(client);
            if (dlg.ShowDialog() == true)
            {
                _dbService.UpdateClient(dlg.Client);
                LoadClients();
            }
        }

        private void EditOrder()
        {
            var order       = (Order)GetSelectedItem()!;
            var carName     = order.CarInfo ?? "—";
            var clientPhone = _dbService.GetClientPhoneById(order.ID_Client);
            var dlg         = new Views.OrderWindow(order, carName, clientPhone);
            if (dlg.ShowDialog() == true)
            {
                _dbService.UpdateOrder(dlg.Order);
                LoadOrders();
            }
        }

        private void EditEmployee()
        {
            var employee = (Employee)GetSelectedItem()!;
            var dlg      = new Views.EmployeeWindow(employee);
            if (dlg.ShowDialog() == true)
            {
                _dbService.UpdateEmployee(dlg.Employee);
                LoadEmployees();
            }
        }

        private void EditDiller()
        {
            var diller = (Diller)GetSelectedItem()!;
            var dlg    = new Views.DillerEditWindow(diller);
            if (dlg.ShowDialog() == true)
            {
                _dbService.UpdateDiller(dlg.Diller);
                LoadDillers();
            }
        }

        

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (!CanEdit()) return;
            if (GetSelectedItem() == null)
            {
                MessageBox.Show("Выберите запись для удаления!", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var confirm = MessageBox.Show("Вы уверены, что хотите удалить запись?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                switch (_currentView)
                {
                    case ViewMode.Cars:      _dbService.DeleteCar(((Car)GetSelectedItem()!).ID_Car);                LoadCars();      break;
                    case ViewMode.Clients:   _dbService.DeleteClient(((Client)GetSelectedItem()!).ID_Client);      LoadClients();   break;
                    case ViewMode.Orders:    _dbService.DeleteOrder(((Order)GetSelectedItem()!).ID_Order);          LoadOrders();    break;
                    case ViewMode.Employees: _dbService.DeleteEmployee(((Employee)GetSelectedItem()!).ID_Employee); LoadEmployees(); break;
                    case ViewMode.Dillers:   _dbService.DeleteDiller(((Diller)GetSelectedItem()!).ID_Diller);      LoadDillers();   break;
                }
            }
            catch (InvalidOperationException ex)
            {
                MessageBox.Show(ex.Message, "Невозможно удалить",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        

        private void ExportToExcel(ViewMode view)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter   = "Excel файл (*.xlsx)|*.xlsx",
                FileName = view == ViewMode.Cars ? "Автомобили" : "Заказы"
            };
            if (dialog.ShowDialog() != true) return;

            try
            {
                if (view == ViewMode.Cars)
                    _dbService.ExportCarsToExcel(dialog.FileName);
                else
                    _dbService.ExportOrdersToExcel(dialog.FileName);

                MessageBox.Show("Файл успешно сохранён!", "Экспорт",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ----------------------
        // Детальная панель
        // ----------------------

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem == null)
            {
                CloseDetail(); return;
            }
            var realItem = dataGrid.SelectedItem is Border b ? b.Tag : dataGrid.SelectedItem;
            if (realItem == null)
            {
                CloseDetail(); return;
            }
            ShowDetail(realItem);
        }

        private void ShowDetail(object item)
        {
            detailBody.Children.Clear();

            detailButtonsBar.Visibility = CanEdit()
                ? Visibility.Visible
                : Visibility.Collapsed;

            switch (_currentView)
            {
                case ViewMode.Cars:
                    var car = (Car)item;
                    detailTitle.Text = $"{car.Mark} {car.Model}";

                    if (!string.IsNullOrEmpty(car.PhotoPath) && System.IO.File.Exists(car.PhotoPath))
                    {
                        try
                        {
                            var bmp = new System.Windows.Media.Imaging.BitmapImage();
                            bmp.BeginInit();
                            bmp.UriSource        = new Uri(car.PhotoPath, UriKind.Absolute);
                            bmp.CacheOption      = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                            bmp.DecodePixelWidth = 580;
                            bmp.EndInit();

                            detailBody.Children.Add(new Border
                            {
                                CornerRadius = new CornerRadius(8),
                                ClipToBounds = true,
                                Margin       = new Thickness(0, 0, 0, 14),
                                Height       = 160,
                                Child = new Image
                                {
                                    Source              = bmp,
                                    Stretch             = System.Windows.Media.Stretch.UniformToFill,
                                    HorizontalAlignment = HorizontalAlignment.Center,
                                    VerticalAlignment   = VerticalAlignment.Center
                                }
                            });
                        }
                        catch { }
                    }

                    AddDetailPrice(car.Price.ToString("N0") + " ₽");
                    AddDetailField("Марка",  car.Mark);
                    AddDetailField("Модель", car.Model);
                    AddDetailField("Цвет",   car.Color);
                    AddDetailField("Год",    car.Year_of_release.ToString());
                    AddDetailSpec(car.Technical_specifications);

                    if (CurrentUser.Role == UserRole.Client)
                        AddDetailBuyButton(car);
                    break;

                case ViewMode.Clients:
                    var client = (Client)item;
                    detailTitle.Text = $"{client.Surname} {client.Name}";
                    AddDetailField("Фамилия",        client.Surname);
                    AddDetailField("Имя",            client.Name);
                    AddDetailField("Отчество",       client.Middle_name ?? "—");
                    AddDetailField("Серия паспорта", client.Passport_series.ToString());
                    AddDetailField("Номер паспорта", client.Passport_number);
                    AddDetailField("Телефон",        client.Phone_number);
                    AddDetailField("Email",          client.Email ?? "—");
                    break;

                case ViewMode.Orders:
                    var order = (Order)item;
                    detailTitle.Text = $"Заказ — {order.ClientName}";
                    AddDetailBadge("Статус",     order.Order_status);
                    AddDetailField("Клиент",     order.ClientName ?? "—");
                    AddDetailField("Автомобиль", order.CarInfo ?? "—");
                    AddDetailField("Дата",       order.Date_of_execution.ToString("dd.MM.yyyy"));
                    AddDetailField("Оплата",     order.Payment_method ?? "—");
                    break;

                case ViewMode.Employees:
                    var emp = (Employee)item;
                    detailTitle.Text = $"{emp.Surname} {emp.Name}";
                    AddDetailField("Фамилия",  emp.Surname);
                    AddDetailField("Имя",      emp.Name);
                    AddDetailField("Отчество", emp.Middle_name ?? "—");
                    AddDetailBadge("Должность", emp.Post);
                    AddDetailField("Телефон",  emp.Business_phone_number);
                    break;

                case ViewMode.Dillers:
                    var diller = (Diller)item;
                    detailTitle.Text = diller.Car_center_name;
                    AddDetailField("Название", diller.Car_center_name);
                    AddDetailField("Телефон",  diller.Phone_number ?? "—");
                    AddDetailField("Email",    diller.Email ?? "—");
                    break;
            }

            detailPanel.Visibility = Visibility.Visible;
        }

        private void btnCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.SelectedItem = null;
            CloseDetail();
        }

       
        
        

        private void AddDetailPrice(string price)
        {
            detailBody.Children.Add(new TextBlock
            {
                Text       = price,
                FontSize   = 22,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(181, 213, 202)),
                Margin     = new Thickness(0, 0, 0, 14)
            });
        }

        private void AddDetailField(string label, string value)
        {
            var wrap = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            wrap.Children.Add(new TextBlock
            {
                Text       = label.ToUpper(),
                FontSize   = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                Margin     = new Thickness(0, 0, 0, 2)
            });
            wrap.Children.Add(new TextBlock
            {
                Text         = value,
                FontSize     = 13,
                FontWeight   = FontWeights.SemiBold,
                Foreground   = new SolidColorBrush(Color.FromRgb(26, 42, 80)),
                TextWrapping = TextWrapping.Wrap
            });
            detailBody.Children.Add(wrap);
        }

        private void AddDetailBadge(string label, string value)
        {
            var wrap = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            wrap.Children.Add(new TextBlock
            {
                Text       = label.ToUpper(),
                FontSize   = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                Margin     = new Thickness(0, 0, 0, 4)
            });

            Color bg, fg;
            var v = value?.ToLower() ?? "";
            if (v.Contains("выполнен") || v.Contains("менедж"))
            { bg = Color.FromRgb(213, 236, 213); fg = Color.FromRgb(26, 80, 32); }
            else if (v.Contains("обработке"))
            { bg = Color.FromRgb(255, 239, 196); fg = Color.FromRgb(122, 80, 0); }
            else if (v.Contains("отмен"))
            { bg = Color.FromRgb(224, 169, 175); fg = Color.FromRgb(112, 43, 19); }
            else
            { bg = Color.FromRgb(0, 0, 0); fg = Color.FromRgb(255, 255, 255); }

            wrap.Children.Add(new Border
            {
                Background          = new SolidColorBrush(bg),
                CornerRadius        = new CornerRadius(6),
                Padding             = new Thickness(10, 4, 10, 4),
                HorizontalAlignment = HorizontalAlignment.Left,
                Child = new TextBlock
                {
                    Text       = value,
                    FontSize   = 12,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(fg)
                }
            });
            detailBody.Children.Add(wrap);
        }

        private void AddDetailSpec(string spec)
        {
            if (string.IsNullOrWhiteSpace(spec)) return;
            var wrap = new StackPanel { Margin = new Thickness(0, 0, 0, 10) };
            wrap.Children.Add(new TextBlock
            {
                Text       = "ТЕХ. ХАРАКТЕРИСТИКИ",
                FontSize   = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                Margin     = new Thickness(0, 0, 0, 4)
            });
            wrap.Children.Add(new Border
            {
                Background      = new SolidColorBrush(Color.FromRgb(255, 252, 214)),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(181, 213, 202)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(8),
                Padding         = new Thickness(10, 8, 10, 8),
                Child = new TextBlock
                {
                    Text         = spec,
                    FontSize     = 12,
                    Foreground   = new SolidColorBrush(Color.FromRgb(58, 80, 128)),
                    TextWrapping = TextWrapping.Wrap
                }
            });
            detailBody.Children.Add(wrap);
        }

      
        
        

        private void AddDetailBuyButton(Car car)
        {
            detailBody.Children.Add(new Border
            {
                Height     = 1,
                Background = new SolidColorBrush(Color.FromRgb(232, 240, 232)),
                Margin     = new Thickness(0, 10, 0, 10)
            });

            var btn = new Button
            {
                Content         = "Купить автомобиль",
                Style = (Style)FindResource("BtnAdd"), 
                Background = new SolidColorBrush(Color.FromRgb(181, 213, 202)),
                Tag = car
            };

            btn.Click += BtnBuy_Click;
            detailBody.Children.Add(btn);
        }

        private void BtnBuy_Click(object sender, RoutedEventArgs e)
        {
            if (((Button)sender).Tag is not Car car) return;

            int clientId = CurrentUser.ClientId;
            if (clientId <= 0)
                clientId = _dbService.GetClientIdByLogin(CurrentUser.Login!);

            if (clientId <= 0)
            {
                MessageBox.Show(
                    "Не найдена запись клиента для вашего аккаунта",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            bool needPassport = !_dbService.ClientHasPassport(clientId);

            var buyWindow = new Views.BuyCarWindow(car, needPassport)
            {
                Owner = this
            };
            if (buyWindow.ShowDialog() != true) 
                return;

            try
            {
                if (buyWindow.PassportWasEntered)
                    _dbService.UpdateClientPassport(clientId, buyWindow.PassportSeries, buyWindow.PassportNumber);

                var order = new Order
                {
                    ID_Client         = clientId,
                    Order_status      = "В обработке",
                    Date_of_execution = DateTime.Now,
                    Payment_method    = buyWindow.PaymentMethod
                };

                _dbService.AddOrder(order, car.ID_Car);

                MessageBox.Show(
                    $"Заказ успешно оформлен!\n\nАвтомобиль: {car.Mark} {car.Model}\nСпособ оплаты: {buyWindow.PaymentMethod}\nСтатус: В обработке",
                    "Заказ оформлен", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка оформления заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}