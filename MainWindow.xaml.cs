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

        private string? _employeePost;

        private string? EmployeePost =>
            _employeePost ??= _dbService.GetEmployeePost(CurrentUser.EmployeeId);

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
                    var post = EmployeePost;

                    if (post == "Менеджер по продажам")
                    {
                        btnOrders.Visibility = Visibility.Visible;
                    }

                    if (post == "Старший менеджер")
                    {
                        btnOrders.Visibility    = Visibility.Visible;
                        btnEmployees.Visibility = Visibility.Visible;
                    }

                    if (post == "Менеджер по работе с клиентами")
                    {
                        btnClients.Visibility = Visibility.Visible;
                        btnOrders.Visibility  = Visibility.Visible;
                    }
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
            if (role == UserRole.Admin) return true;

            if (role == UserRole.Employee)
            {
                var post = EmployeePost;
                return _currentView switch
                {
                    ViewMode.Cars =>
                        post == "Менеджер по продажам" ||
                        post == "Старший менеджер",

                    ViewMode.Clients =>
                        post == "Старший менеджер" ||
                        post == "Менеджер по работе с клиентами",

                    ViewMode.Orders =>
                        post == "Менеджер по продажам" ||
                        post == "Старший менеджер",

                    ViewMode.Employees =>
                        post == "Старший менеджер",

                    _ => false
                };
            }
            return false;
        }

        
        

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

       
        
        
        private static readonly string[] _avatarColors =
        {
            "#B5D5CA","#1D9E75","#993C1D","#854F0B","#534AB7",
            "#0F6E56","#D85A30","#185FA5","#3B6D11","#BA7517",
            "#993556","#5F5E5A","#A32D2D"
        };

        private string AvatarColor(int index) =>
            _avatarColors[index % _avatarColors.Length];

        private static string Initials(params string?[] parts)
        {
            var result = "";
            foreach (var p in parts)
                if (!string.IsNullOrWhiteSpace(p)) result += p.Trim()[0];
            return result.ToUpper().Length > 2 ? result.ToUpper()[..2] : result.ToUpper();
        }

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
                Tag             = dataItem,
                Width           = 260
            };

            var stack = new StackPanel
            {
                Margin = new Thickness(12, 10, 12, 10)
            };

            var header = new StackPanel
            {
                Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 8)
            };
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
                    Text       = subtitle,
                    FontSize   = 11,
                    Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                    Margin     = new Thickness(0, 1, 0, 0)
                });

            header.Children.Add(avatar);
            header.Children.Add(titleBlock);
            stack.Children.Add(header);

            stack.Children.Add(new Border
            {
                Height     = 1,
                Background = new SolidColorBrush(Color.FromRgb(238, 242, 250)),
                Margin     = new Thickness(0, 0, 0, 8)
            });

            foreach (var (key, val) in fields)
            {
                var row = new Grid { Margin = new Thickness(0, 0, 0, 4) };
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var keyTb = new TextBlock
                {
                    Text       = key,
                    FontSize   = 11,
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

            outerBorder.MouseEnter += (s, e) =>
                ((Border)s).Background = new SolidColorBrush(Color.FromRgb(209, 238, 252));
            outerBorder.MouseLeave += (s, e) =>
                ((Border)s).Background = Brushes.White;
            outerBorder.MouseLeftButtonUp += (s, e) =>
            {
                var item = ((Border)s).Tag;
                dataGrid.SelectedItem = item;
            };

            return outerBorder;
        }

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

            var photoGrid = new Grid { Height = 195 };

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
                        Source              = bmp,
                        Stretch             = System.Windows.Media.Stretch.UniformToFill,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment   = VerticalAlignment.Center
                    });
                }
                catch { hasPhoto = false; }
            }

            if (!hasPhoto)
            {
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

            var body = new StackPanel { Margin = new Thickness(12, 10, 12, 12) };
            body.Children.Add(new TextBlock
            {
                Text       = $"{car.Mark} {car.Model}",
                FontSize   = 13,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(26, 42, 80))
            });
            body.Children.Add(new TextBlock
            {
                Text       = $"{car.Year_of_release} · {car.Color}",
                FontSize   = 11,
                Foreground = new SolidColorBrush(Color.FromRgb(138, 154, 181)),
                Margin     = new Thickness(0, 2, 0, 0)
            });
            body.Children.Add(new TextBlock
            {
                Text       = $"{car.Price:N0} ₽",
                FontSize   = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(181, 213, 202)),
                Margin     = new Thickness(0, 6, 0, 6)
            });

            var chips = new WrapPanel { Orientation = Orientation.Horizontal };
            foreach (var chip in ParseChips(car.Technical_specifications))
                chips.Children.Add(MakeChip(chip));
            body.Children.Add(chips);

            stack.Children.Add(body);
            outer.Child = stack;

            outer.MouseEnter += (s, e) =>
                ((Border)s).Background = new SolidColorBrush(Color.FromRgb(209, 238, 252));
            outer.MouseLeave += (s, e) =>
                ((Border)s).Background = Brushes.White;
            outer.MouseLeftButtonUp += (s, e) =>
                dataGrid.SelectedItem = outer;

            return outer;
        }

        private static string[] ParseChips(string tech)
        {
            if (string.IsNullOrWhiteSpace(tech)) return [];
            var parts  = tech.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var result = new List<string>();
            foreach (var p in parts)
            {
                var t = p.Trim();
                if (t.Length > 0 && result.Count < 3)
                    result.Add(t);
            }
            return result.ToArray();
        }

        private static Border MakeChip(string text) => new Border
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

        private void FillCards<T>(IEnumerable<T> items, Func<T, int, Border> makeCard)
        {
            dataGrid.Items.Clear();
            int i = 0;
            foreach (var item in items)
                dataGrid.Items.Add(makeCard(item, i++));
        }

        private void UpdateActionButtons()
        {
            bool canEdit = CanEdit();

            btnAdd.Visibility    = canEdit ? Visibility.Visible : Visibility.Collapsed;
            btnEdit.Visibility   = canEdit ? Visibility.Visible : Visibility.Collapsed;
            btnDelete.Visibility = canEdit ? Visibility.Visible : Visibility.Collapsed;

            btnAdd.IsEnabled    = canEdit;
            btnEdit.IsEnabled   = canEdit;
            btnDelete.IsEnabled = canEdit;

            btnReports.Visibility = CurrentUser.Role == UserRole.Admin
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void CloseDetail()
        {
            detailPanel.Visibility = Visibility.Collapsed;
        }
        
        
       
        

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

        private object? GetSelectedItem() =>
            dataGrid.SelectedItem is Border b ? b.Tag : dataGrid.SelectedItem;
    }
}