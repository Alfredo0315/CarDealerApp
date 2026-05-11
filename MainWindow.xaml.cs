using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using CarDealerApp.Models;
using CarDealerApp.Services;
using Microsoft.Win32;
using System.Windows.Media;
using CarDealerApp.Views;

namespace CarDealerApp
{
    public partial class MainWindow : Window
    {
        private readonly DatabaseService _dbService;

        private enum ViewMode { Cars, Clients, Orders, Employees, Dillers }
        private ViewMode _currentView = ViewMode.Cars;

        public MainWindow()
        {
            InitializeComponent();
            _dbService = new DatabaseService();

            txtUserName.Text = CurrentUser.Login ?? "";
            txtUserRole.Text = $"Роль: {CurrentUser.Role}";

            ApplyRoleMenuVisibility();
            LoadDefaultView();
        }

        // ----------------------
        // Роли и доступ
        // ----------------------

        private void ApplyRoleMenuVisibility()
        {
            var role = CurrentUser.Role;

            btnClients.Visibility   = Visibility.Collapsed;
            btnOrders.Visibility    = Visibility.Collapsed;
            btnEmployees.Visibility = Visibility.Collapsed;
            btnDillers.Visibility   = Visibility.Collapsed;

            switch (role)
            {
                case UserRole.Admin:
                    btnClients.Visibility   = Visibility.Visible;
                    btnOrders.Visibility    = Visibility.Visible;
                    btnEmployees.Visibility = Visibility.Visible;
                    btnDillers.Visibility   = Visibility.Visible;
                    break;
                case UserRole.Employee:
                    btnOrders.Visibility    = Visibility.Visible;
                    btnEmployees.Visibility = Visibility.Visible;
                    break;
                case UserRole.Diller:
                    btnDillers.Visibility = Visibility.Visible;
                    break;
                case UserRole.Client:
                    btnReports.Visibility = Visibility.Collapsed;
                    break;
            }
        }

        private void LoadDefaultView() => LoadCars();

        private bool CanEdit()
        {
            var role = CurrentUser.Role;
            return _currentView switch
            {
                ViewMode.Cars      => role == UserRole.Admin || role == UserRole.Employee || role == UserRole.Diller,
                ViewMode.Clients   => role == UserRole.Admin,
                ViewMode.Orders    => role == UserRole.Admin || role == UserRole.Employee,
                ViewMode.Employees => role == UserRole.Admin || role == UserRole.Employee,
                ViewMode.Dillers   => role == UserRole.Admin || role == UserRole.Diller,
                _ => false
            };
        }

        // ----------------------
        // Загрузка данных
        // ----------------------

        private void LoadCars()
        {
            _currentView = ViewMode.Cars;
            txtTitle.Text = "Автомобили";
            var items = _dbService.GetAllCars();
            dataGrid.Items.Clear();
            int i = 0;
            foreach (var car in items)
            {
                var card = MakeCarCard(car, i++);
                dataGrid.Items.Add(card);
            }
            UpdateActionButtons();
            CloseDetail();
        }

        private void LoadClients()
        {
            _currentView = ViewMode.Clients;
            txtTitle.Text = "Клиенты";
            var items = _dbService.GetAllClients();
            FillCards(items, (c, i) => MakeCard(c,
                Initials(c.Surname, c.Name),
                AvatarColor(i),
                $"{c.Surname} {c.Name}",
                c.Middle_name ?? "",
                ("Телефон", c.Phone_number),
                ("Email",   c.Email ?? "—")
            ));
            UpdateActionButtons();
            CloseDetail();
        }

        private void LoadOrders()
        {
            _currentView = ViewMode.Orders;
            txtTitle.Text = "Заказы";
            var items = _dbService.GetAllOrders();
            FillCards(items, (o, i) => MakeCard(o,
                Initials(o.ClientName),
                AvatarColor(i),
                o.ClientName ?? "—",
                o.CarInfo ?? "—",
                ("Статус", o.Order_status),
                ("Дата",   o.Date_of_execution.ToString("dd.MM.yyyy")),
                ("Оплата", o.Payment_method ?? "—")
            ));
            UpdateActionButtons();
            CloseDetail();
        }

        private void LoadEmployees()
        {
            _currentView = ViewMode.Employees;
            txtTitle.Text = "Сотрудники";
            var items = _dbService.GetAllEmployees();
            FillCards(items, (emp, i) => MakeCard(emp,
                Initials(emp.Surname, emp.Name),
                AvatarColor(i),
                $"{emp.Surname} {emp.Name}",
                emp.Post,
                ("Телефон", emp.Business_phone_number)
            ));
            UpdateActionButtons();
            CloseDetail();
        }

        private void LoadDillers()
        {
            _currentView = ViewMode.Dillers;
            txtTitle.Text = "Дилеры";
            var items = _dbService.GetAllDillers();
            FillCards(items, (d, i) => MakeCard(d,
                Initials(d.Car_center_name),
                AvatarColor(i),
                d.Car_center_name,
                d.Email ?? "",
                ("Телефон", d.Phone_number ?? "—"),
                ("Email",   d.Email ?? "—")
            ));
            UpdateActionButtons();
            CloseDetail();
        }

        // ----------------------
        // Поиск
        // ----------------------

        private void txtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = txtSearch.Text.Trim().ToLower();

            switch (_currentView)
            {
                case ViewMode.Cars:
                    var cars = _dbService.GetAllCars();
                    if (!string.IsNullOrEmpty(query))
                        cars = cars.Where(c =>
                            c.Mark.ToLower().Contains(query) ||
                            c.Model.ToLower().Contains(query) ||
                            c.Color.ToLower().Contains(query) ||
                            c.Year_of_release.ToString().Contains(query)).ToList();
                    dataGrid.Items.Clear();
                    int idx = 0;
                    foreach (var car in cars)
                        dataGrid.Items.Add(MakeCarCard(car, idx++));
                    break;

                case ViewMode.Clients:
                    var clients = _dbService.GetAllClients();
                    if (!string.IsNullOrEmpty(query))
                        clients = clients.Where(c =>
                            c.Surname.ToLower().Contains(query) ||
                            c.Name.ToLower().Contains(query) ||
                            (c.Phone_number ?? "").ToLower().Contains(query) ||
                            (c.Email ?? "").ToLower().Contains(query)).ToList();
                    FillCards(clients, (c, i) => MakeCard(c, Initials(c.Surname, c.Name), AvatarColor(i),
                        $"{c.Surname} {c.Name}", c.Middle_name ?? "",
                        ("Телефон", c.Phone_number), ("Email", c.Email ?? "—")));
                    break;

                case ViewMode.Orders:
                    var orders = _dbService.GetAllOrders();
                    if (!string.IsNullOrEmpty(query))
                        orders = orders.Where(o =>
                            (o.ClientName ?? "").ToLower().Contains(query) ||
                            (o.CarInfo ?? "").ToLower().Contains(query) ||
                            (o.Order_status ?? "").ToLower().Contains(query) ||
                            (o.Payment_method ?? "").ToLower().Contains(query)).ToList();
                    FillCards(orders, (o, i) => MakeCard(o, Initials(o.ClientName), AvatarColor(i),
                        o.ClientName ?? "—", o.CarInfo ?? "—",
                        ("Статус", o.Order_status), ("Дата", o.Date_of_execution.ToString("dd.MM.yyyy")),
                        ("Оплата", o.Payment_method ?? "—")));
                    break;

                case ViewMode.Employees:
                    var employees = _dbService.GetAllEmployees();
                    if (!string.IsNullOrEmpty(query))
                        employees = employees.Where(emp =>
                            emp.Surname.ToLower().Contains(query) ||
                            emp.Name.ToLower().Contains(query) ||
                            (emp.Post ?? "").ToLower().Contains(query) ||
                            (emp.Business_phone_number ?? "").ToLower().Contains(query)).ToList();
                    FillCards(employees, (emp, i) => MakeCard(emp, Initials(emp.Surname, emp.Name), AvatarColor(i),
                        $"{emp.Surname} {emp.Name}", emp.Post, ("Телефон", emp.Business_phone_number)));
                    break;

                case ViewMode.Dillers:
                    var dillers = _dbService.GetAllDillers();
                    if (!string.IsNullOrEmpty(query))
                        dillers = dillers.Where(d =>
                            (d.Car_center_name ?? "").ToLower().Contains(query) ||
                            (d.Email ?? "").ToLower().Contains(query) ||
                            (d.Phone_number ?? "").ToLower().Contains(query)).ToList();
                    FillCards(dillers, (d, i) => MakeCard(d, Initials(d.Car_center_name), AvatarColor(i),
                        d.Car_center_name, d.Email ?? "",
                        ("Телефон", d.Phone_number ?? "—"), ("Email", d.Email ?? "—")));
                    break;
            }
            CloseDetail();
        }

        // Получить реальный объект из выбранного элемента ListBox
        private object? GetSelectedItem() =>
            dataGrid.SelectedItem is Border b ? b.Tag : dataGrid.SelectedItem;

        // ----------------------
        // Настройка колонок таблиц
        // ----------------------

        // Цвета аватаров — по индексу записи
        private static readonly string[] _avatarColors =
        {
            "#B5D5CA","#1D9E75","#993C1D","#854F0B","#534AB7",
            "#0F6E56","#D85A30","#185FA5","#3B6D11","#BA7517",
            "#993556","#5F5E5A","#A32D2D"
        };

        private string AvatarColor(int index) =>
            _avatarColors[index % _avatarColors.Length];

        // Первые буквы для аватара
        private static string Initials(params string?[] parts)
        {
            var result = "";
            foreach (var p in parts)
                if (!string.IsNullOrWhiteSpace(p)) result += p.Trim()[0];
            return result.ToUpper().Length > 2 ? result.ToUpper()[..2] : result.ToUpper();
        }

        // Строит карточку для ListBox
        private Border MakeCard(object dataItem, string avatarText, string avatarColor,
                                string title, string subtitle, params (string Key, string Val)[] fields)
        {
            var outerBorder = new Border
            {
                Background      = Brushes.White,
                CornerRadius    = new CornerRadius(10),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(224, 234, 246)),
                BorderThickness = new Thickness(1),
                Margin          = new Thickness(4),
                Cursor          = Cursors.Hand,
                Tag             = dataItem
            };

            // Ширина: половина доступного места минус отступы
            outerBorder.Width = 260;

            var stack = new StackPanel { Margin = new Thickness(12, 10, 12, 10) };

            // Шапка: аватар + название
            var header = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8) };
            var avatar = new Border
            {
                Width        = 36,
                Height       = 36,
                CornerRadius = new CornerRadius(9),
                Background   = new SolidColorBrush((Color)ColorConverter.ConvertFromString(avatarColor)),
                Margin       = new Thickness(0, 0, 10, 0)
            };
            avatar.Child = new TextBlock
            {
                Text                = avatarText,
                FontSize            = 13,
                FontWeight          = FontWeights.SemiBold,
                Foreground          = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment   = VerticalAlignment.Center
            };

            var titleBlock = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            titleBlock.Children.Add(new TextBlock
            {
                Text         = title,
                FontSize     = 13,
                FontWeight   = FontWeights.SemiBold,
                Foreground   = new SolidColorBrush(Color.FromRgb(26, 42, 80)),
                TextWrapping = TextWrapping.Wrap,
                MaxWidth     = 190
            });
            if (!string.IsNullOrEmpty(subtitle))
                titleBlock.Children.Add(new TextBlock
                {
                    Text      = subtitle,
                    FontSize  = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                    Margin    = new Thickness(0, 1, 0, 0)
                });

            header.Children.Add(avatar);
            header.Children.Add(titleBlock);
            stack.Children.Add(header);

            // Разделитель
            stack.Children.Add(new Border
            {
                Height     = 1,
                Background = new SolidColorBrush(Color.FromRgb(238, 242, 250)),
                Margin     = new Thickness(0, 0, 0, 8)
            });

            // Поля
            foreach (var (key, val) in fields)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 4) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var keyTb = new TextBlock
                {
                    Text      = key,
                    FontSize  = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(170, 181, 204))
                };
                var valTb = new TextBlock
                {
                    Text                = val,
                    FontSize            = 11,
                    FontWeight          = FontWeights.SemiBold,
                    Foreground          = new SolidColorBrush(Color.FromRgb(42, 58, 90)),
                    TextAlignment       = TextAlignment.Right,
                    TextWrapping        = TextWrapping.Wrap,
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                Grid.SetColumn(keyTb, 0);
                Grid.SetColumn(valTb, 1);
                row.Children.Add(keyTb);
                row.Children.Add(valTb);
                stack.Children.Add(row);
            }

            outerBorder.Child = stack;

            // Hover-эффект
            outerBorder.MouseEnter += (s, e) =>
                ((Border)s).Background = new SolidColorBrush(Color.FromRgb(209, 238, 252));
            outerBorder.MouseLeave += (s, e) =>
                ((Border)s).Background = Brushes.White;
            outerBorder.MouseLeftButtonUp += (s, e) =>
            {
                // Выделяем в ListBox
                var item = ((Border)s).Tag;
                dataGrid.SelectedItem = item;
            };

            return outerBorder;
        }

        // Карточка автомобиля с фото (4:3) сверху
        private Border MakeCarCard(Car car, int index)
        {
            var outer = new Border
            {
                Background      = Brushes.White,
                CornerRadius    = new CornerRadius(12),
                BorderBrush     = new SolidColorBrush(Color.FromRgb(224, 234, 246)),
                BorderThickness = new Thickness(1),
                Margin          = new Thickness(4),
                Width           = 260,
                Cursor          = Cursors.Hand,
                Tag             = car,
                ClipToBounds    = true
            };

            var stack = new StackPanel();

            // Блок фото (пропорции 4:3)
            var photoGrid = new Grid
            {
                Height = 195  // 260 * 3/4 = 195
            };

            bool hasPhoto = !string.IsNullOrEmpty(car.PhotoPath) &&
                            System.IO.File.Exists(car.PhotoPath);

            if (hasPhoto)
            {
                try
                {
                    var bmp = new System.Windows.Media.Imaging.BitmapImage();
                    bmp.BeginInit();
                    bmp.UriSource        = new Uri(car.PhotoPath!, UriKind.Absolute);
                    bmp.CacheOption      = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bmp.DecodePixelWidth = 520;
                    bmp.EndInit();

                    photoGrid.Children.Add(new Image
                    {
                        Source  = bmp,
                        Stretch = System.Windows.Media.Stretch.UniformToFill,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center
                    });
                }
                catch
                {
                    hasPhoto = false;
                }
            }

            if (!hasPhoto)
            {
                // Заглушка с цветным фоном и инициалами
                photoGrid.Background = new SolidColorBrush(
                    (Color)ColorConverter.ConvertFromString("#D1EEFC"));

                var stub = new StackPanel
                {
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment   = VerticalAlignment.Center
                };
                stub.Children.Add(new TextBlock
                {
                    Text                = "🚗",
                    FontSize            = 44,
                    HorizontalAlignment = HorizontalAlignment.Center
                });
                stub.Children.Add(new TextBlock
                {
                    Text                = $"{car.Mark} {car.Model}",
                    FontSize            = 12,
                    FontWeight          = FontWeights.SemiBold,
                    Foreground          = new SolidColorBrush(Color.FromRgb(181, 213, 202)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin              = new Thickness(0, 6, 0, 0)
                });
                photoGrid.Children.Add(stub);
            }

            stack.Children.Add(photoGrid);

            // Тело карточки
            var body = new StackPanel { Margin = new Thickness(12, 10, 12, 12) };

            body.Children.Add(new TextBlock
            {
                Text         = $"{car.Mark} {car.Model}",
                FontSize     = 13,
                FontWeight   = FontWeights.SemiBold,
                Foreground   = new SolidColorBrush(Color.FromRgb(26, 42, 80))
            });
            body.Children.Add(new TextBlock
            {
                Text      = $"{car.Year_of_release} · {car.Color}",
                FontSize  = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                Margin    = new Thickness(0, 2, 0, 0)
            });
            body.Children.Add(new TextBlock
            {
                Text       = $"{car.Price:N0} ₽",
                FontSize   = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(181, 213, 202)),
                Margin     = new Thickness(0, 6, 0, 6)
            });

            // Чипы характеристик
            var chips = new WrapPanel { Orientation = Orientation.Horizontal };
            foreach (var chip in ParseChips(car.Technical_specifications))
                chips.Children.Add(MakeChip(chip));
            body.Children.Add(chips);

            stack.Children.Add(body);
            outer.Child = stack;

            // Hover
            outer.MouseEnter += (s, e) =>
                ((Border)s).Background = new SolidColorBrush(Color.FromRgb(209, 238, 252));
            outer.MouseLeave += (s, e) =>
                ((Border)s).Background = Brushes.White;
            outer.MouseLeftButtonUp += (s, e) =>
                dataGrid.SelectedItem = outer;

            return outer;
        }

        // Разбирает строку тех. характеристик на чипы (до 3 штук)
        private static string[] ParseChips(string tech)
        {
            if (string.IsNullOrWhiteSpace(tech)) return [];
            var parts = tech.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new System.Collections.Generic.List<string>();
            foreach (var p in parts)
            {
                var t = p.Trim();
                if (t.Length > 0 && result.Count < 3)
                    result.Add(t);
            }
            return result.ToArray();
        }

        // Создаёт один чип
        private static Border MakeChip(string text)
        {
            return new Border
            {
                Background      = new SolidColorBrush(Color.FromRgb(209, 238, 252)),
                CornerRadius    = new CornerRadius(5),
                Padding         = new Thickness(7, 2, 7, 2),
                Margin          = new Thickness(0, 0, 4, 4),
                Child = new TextBlock
                {
                    Text       = text,
                    FontSize   = 10,
                    FontWeight = FontWeights.SemiBold,
                    Foreground = new SolidColorBrush(Color.FromRgb(26, 58, 40))
                }
            };
        }
        private void SetupClientsGrid() { }
        private void SetupOrdersGrid() { }
        private void SetupEmployeesGrid() { }
        private void SetupDillersGrid() { }

        // Заполняет ListBox карточками
        private void FillCards<T>(IEnumerable<T> items, Func<T, int, Border> makeCard)
        {
            dataGrid.Items.Clear();
            int i = 0;
            foreach (var item in items)
                dataGrid.Items.Add(makeCard(item, i++));
        }

        // ----------------------
        // Кнопки действий
        // ----------------------

        private void UpdateActionButtons()
        {
            bool canEdit = CanEdit();

            // Клиент не видит кнопки вообще — скрываем
            btnAdd.Visibility    = canEdit ? Visibility.Visible : Visibility.Collapsed;
            btnEdit.Visibility   = canEdit ? Visibility.Visible : Visibility.Collapsed;
            btnDelete.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            btnAdd.IsEnabled    = canEdit;
            btnEdit.IsEnabled   = canEdit;
            btnDelete.IsEnabled = canEdit;

            // Excel только для Admin
            btnReports.Visibility = CurrentUser.Role == UserRole.Admin
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

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

        // ----------------------
        // Добавление
        // ----------------------

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
            if (dlg.ShowDialog() == true) { _dbService.AddCar(dlg.Car); LoadCars(); }
        }

        private void AddClient()
        {
            var dlg = new Views.ClientWindow();
            if (dlg.ShowDialog() == true) { _dbService.AddClient(dlg.Client); LoadClients(); }
        }

        private void AddOrder()
        {
            var dlg = new Views.OrderWindow();
            if (dlg.ShowDialog() == true) { _dbService.AddOrder(dlg.Order, dlg.CarId); LoadOrders(); }
        }

        private void AddEmployee()
        {
            var dlg = new Views.EmployeeWindow();
            if (dlg.ShowDialog() == true) { _dbService.AddEmployee(dlg.Employee); LoadEmployees(); }
        }

        private void AddDiller()
        {
            var dlg = new Views.DillerEditWindow();
            if (dlg.ShowDialog() == true) { _dbService.AddDiller(dlg.Diller); LoadDillers(); }
        }

        // ----------------------
        // Редактирование
        // ----------------------

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
            if (dlg.ShowDialog() == true) { _dbService.UpdateCar(dlg.Car); LoadCars(); }
        }

        private void EditClient()
        {
            var client = (Client)GetSelectedItem()!;
            var dlg = new Views.ClientWindow(client);
            if (dlg.ShowDialog() == true) { _dbService.UpdateClient(dlg.Client); LoadClients(); }
        }

        private void EditOrder()
        {
            var order = (Order)GetSelectedItem()!;
            var dlg = new Views.OrderWindow(order);
            if (dlg.ShowDialog() == true) { _dbService.UpdateOrder(dlg.Order); LoadOrders(); }
        }

        private void EditEmployee()
        {
            var employee = (Employee)GetSelectedItem()!;
            var dlg = new Views.EmployeeWindow(employee);
            if (dlg.ShowDialog() == true) { _dbService.UpdateEmployee(dlg.Employee); LoadEmployees(); }
        }

        private void EditDiller()
        {
            var diller = (Diller)GetSelectedItem()!;
            var dlg = new Views.DillerEditWindow(diller);
            if (dlg.ShowDialog() == true) { _dbService.UpdateDiller(dlg.Diller); LoadDillers(); }
        }

        // ----------------------
        // Удаление
        // ----------------------

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
                    case ViewMode.Cars:
                        _dbService.DeleteCar(((Car)GetSelectedItem()!).ID_Car);
                        LoadCars(); break;
                    case ViewMode.Clients:
                        _dbService.DeleteClient(((Client)GetSelectedItem()!).ID_Client);
                        LoadClients(); break;
                    case ViewMode.Orders:
                        _dbService.DeleteOrder(((Order)GetSelectedItem()!).ID_Order);
                        LoadOrders(); break;
                    case ViewMode.Employees:
                        _dbService.DeleteEmployee(((Employee)GetSelectedItem()!).ID_Employee);
                        LoadEmployees(); break;
                    case ViewMode.Dillers:
                        _dbService.DeleteDiller(((Diller)GetSelectedItem()!).ID_Diller);
                        LoadDillers(); break;
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

        // ----------------------
        // Экспорт
        // ----------------------

        private void btnExport_Click(object sender, RoutedEventArgs e) => ExportToExcel(_currentView);

        private void ExportToExcel(ViewMode view)
        {
            var dialog = new SaveFileDialog
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
        // Мастер-деталь: карточка
        // ----------------------

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem == null) { CloseDetail(); return; }
            // SelectedItem — это Border-карточка, реальный объект в Tag
            var realItem = dataGrid.SelectedItem is Border b ? b.Tag : dataGrid.SelectedItem;
            if (realItem == null) { CloseDetail(); return; }
            ShowDetail(realItem);
        }

        private void ShowDetail(object item)
        {
            detailBody.Children.Clear();

            // Кнопки Удалить/Изменить скрыты для клиента
            detailButtonsBar.Visibility = CanEdit()
                ? Visibility.Visible
                : Visibility.Collapsed;

            switch (_currentView)
            {
                case ViewMode.Cars:
                    var car = (Car)item;
                    detailTitle.Text = $"{car.Mark} {car.Model}";

                    // Фото в карточке детали
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
                                CornerRadius    = new CornerRadius(8),
                                ClipToBounds    = true,
                                Margin          = new Thickness(0, 0, 0, 14),
                                Height          = 160,
                                Child = new Image
                                {
                                    Source  = bmp,
                                    Stretch = System.Windows.Media.Stretch.UniformToFill,
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

                    // Логин — только для Admin
                    if (CurrentUser.Role == UserRole.Admin)
                    {
                        var loginC = client.ID_User.HasValue
                            ? _dbService.GetLoginByUserId(client.ID_User.Value)
                            : null;
                        AddDetailLogin(loginC);
                    }
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

                    // Логин — для Admin и Employee
                    if (CurrentUser.Role == UserRole.Admin || CurrentUser.Role == UserRole.Employee)
                    {
                        var loginE = emp.ID_User.HasValue
                            ? _dbService.GetLoginByUserId(emp.ID_User.Value)
                            : null;
                        AddDetailLogin(loginE);
                    }
                    break;

                case ViewMode.Dillers:
                    var diller = (Diller)item;
                    detailTitle.Text = diller.Car_center_name;
                    AddDetailField("Название", diller.Car_center_name);
                    AddDetailField("Телефон",  diller.Phone_number ?? "—");
                    AddDetailField("Email",    diller.Email ?? "—");

                    // Логин — только для Admin
                    if (CurrentUser.Role == UserRole.Admin)
                    {
                        var loginD = diller.ID_User.HasValue
                            ? _dbService.GetLoginByUserId(diller.ID_User.Value)
                            : null;
                        AddDetailLogin(loginD);
                    }
                    break;
            }

            detailPanel.Visibility = Visibility.Visible;
        }

        private void CloseDetail()
        {
            detailPanel.Visibility = Visibility.Collapsed;
        }

        private void btnCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            dataGrid.SelectedItem = null;
            CloseDetail();
        }

        // ----------------------
        // Поля карточки детали
        // ----------------------

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

        // Поле логина с иконкой — видно только Админу
        private void AddDetailLogin(string? login)
        {
            var wrap = new StackPanel { Margin = new Thickness(0, 4, 0, 10) };

            wrap.Children.Add(new TextBlock
            {
                Text       = "ЛОГИН В СИСТЕМЕ",
                FontSize   = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                Margin     = new Thickness(0, 0, 0, 4)
            });

            bool hasLogin = !string.IsNullOrEmpty(login);

            var row = new StackPanel { Orientation = Orientation.Horizontal };
            row.Children.Add(new TextBlock
            {
                Text              = hasLogin ? "🔑 " : "🔒 ",
                FontSize          = 13,
                VerticalAlignment = VerticalAlignment.Center
            });
            row.Children.Add(new TextBlock
            {
                Text              = hasLogin ? login! : "Не зарегистрирован",
                FontSize          = 12,
                FontWeight        = hasLogin ? FontWeights.SemiBold : FontWeights.Normal,
                Foreground        = hasLogin
                    ? new SolidColorBrush(Color.FromRgb(26, 58, 40))
                    : new SolidColorBrush(Color.FromRgb(160, 160, 180)),
                FontStyle         = hasLogin ? FontStyles.Normal : FontStyles.Italic,
                VerticalAlignment = VerticalAlignment.Center
            });

            wrap.Children.Add(new Border
            {
                Background      = hasLogin
                    ? new SolidColorBrush(Color.FromRgb(209, 238, 252))
                    : new SolidColorBrush(Color.FromRgb(245, 245, 248)),
                BorderBrush     = hasLogin
                    ? new SolidColorBrush(Color.FromRgb(181, 213, 202))
                    : new SolidColorBrush(Color.FromRgb(220, 220, 228)),
                BorderThickness = new Thickness(1),
                CornerRadius    = new CornerRadius(7),
                Padding         = new Thickness(10, 7, 10, 7),
                Child           = row
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
            if (v.Contains("выполнен") || v.Contains("директор") || v.Contains("старший"))
            { bg = Color.FromRgb(213, 236, 213); fg = Color.FromRgb(26, 80, 32); }
            else if (v.Contains("обработке") || v.Contains("администратор") || v.Contains("ожидает"))
            { bg = Color.FromRgb(255, 239, 196); fg = Color.FromRgb(122, 80, 0); }
            else if (v.Contains("отмен") || v.Contains("консультант"))
            { bg = Color.FromRgb(224, 169, 175); fg = Color.FromRgb(112, 43, 19); }
            else
            { bg = Color.FromRgb(209, 238, 252); fg = Color.FromRgb(26, 58, 40); }

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

    }
}