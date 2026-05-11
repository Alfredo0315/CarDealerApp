using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using CarDealerApp.Models;
using Microsoft.Data.SqlClient;

namespace CarDealerApp.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            _connectionString = "Server=DESKTOP-4H4B3M6\\SQLEXPRESS;Database=Car_dealer;Trusted_Connection=True;TrustServerCertificate=True;";
        }

        // ============================
        // Авторизация и регистрация
        // ============================

        public bool Authenticate(string login, string password, out UserRole? role)
        {
            role = null;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                var query = "SELECT Role, PasswordHash FROM Users WHERE Login = @login AND IsActive = 1";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login", login);

                using var reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    var dbRole     = reader.GetString(0);
                    var dbPassword = reader.GetString(1);

                    if (dbPassword == password)
                    {
                        role = Enum.Parse<UserRole>(dbRole);
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка авторизации: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        public bool RegisterUser(string login, string password, string role, out string error)
        {
            error = string.Empty;

            if (!ValidatePassword(password, out error))
                return false;

            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();

                var checkQuery = "SELECT COUNT(*) FROM Users WHERE Login = @login";
                using var checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@login", login);

                if ((int)checkCmd.ExecuteScalar() > 0)
                {
                    error = "Пользователь с таким логином уже существует!";
                    return false;
                }

                var query = @"INSERT INTO Users (Login, PasswordHash, Role, IsActive)
                              VALUES (@login, @password, @role, 1)";
                using var cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@login",    login);
                cmd.Parameters.AddWithValue("@password", password);
                cmd.Parameters.AddWithValue("@role",     role);
                cmd.ExecuteNonQuery();
                return true;
            }
            catch (Exception ex)
            {
                error = $"Ошибка регистрации: {ex.Message}";
                return false;
            }
        }

        public static bool ValidatePassword(string password, out string error)
        {
            error = string.Empty;
            if (password.Length < 6)
                error = "Пароль должен содержать минимум 6 символов";
            else if (!password.Any(char.IsUpper))
                error = "Пароль должен содержать минимум 1 прописную букву";
            else if (!password.Any(char.IsDigit))
                error = "Пароль должен содержать минимум 1 цифру";
            else if (!password.Any(c => "!@#$%^".Contains(c)))
                error = "Пароль должен содержать минимум 1 символ из: ! @ # $ % ^";

            return string.IsNullOrEmpty(error);
        }

        // Получить логин пользователя по ID — для отображения в карточке (только Admin)
        public string? GetLoginByUserId(int userId)
        {
            try
            {
                using var conn = new SqlConnection(_connectionString);
                conn.Open();
                using var cmd = new SqlCommand(
                    "SELECT Login FROM Users WHERE ID_User = @id", conn);
                cmd.Parameters.AddWithValue("@id", userId);
                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
            catch
            {
                return null;
            }
        }

        // ============================
        // CRUD Автомобили
        // ============================

        public List<Car> GetAllCars()
        {
            var cars = new List<Car>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = "SELECT ID_Car, Mark, Model, Color, Year_of_release, Price, Technical_specifications, PhotoPath FROM Car";
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                cars.Add(new Car
                {
                    ID_Car                   = reader.GetInt32(0),
                    Mark                     = reader.GetString(1),
                    Model                    = reader.GetString(2),
                    Color                    = reader.GetString(3),
                    Year_of_release          = reader.GetInt32(4),
                    Price                    = reader.GetDecimal(5),
                    Technical_specifications = reader.GetString(6),
                    PhotoPath                = reader.IsDBNull(7) ? null : reader.GetString(7)
                });
            }
            return cars;
        }

        public void AddCar(Car car)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"INSERT INTO Car (Mark, Model, Color, Year_of_release, Price, Technical_specifications, PhotoPath)
                          VALUES (@mark, @model, @color, @year, @price, @tech, @photo)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@mark",  car.Mark);
            cmd.Parameters.AddWithValue("@model", car.Model);
            cmd.Parameters.AddWithValue("@color", car.Color);
            cmd.Parameters.AddWithValue("@year",  car.Year_of_release);
            cmd.Parameters.AddWithValue("@price", car.Price);
            cmd.Parameters.AddWithValue("@tech",  car.Technical_specifications);
            cmd.Parameters.AddWithValue("@photo", (object?)car.PhotoPath ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void UpdateCar(Car car)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"UPDATE Car SET Mark=@mark, Model=@model, Color=@color,
                          Year_of_release=@year, Price=@price, Technical_specifications=@tech,
                          PhotoPath=@photo
                          WHERE ID_Car=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id",    car.ID_Car);
            cmd.Parameters.AddWithValue("@mark",  car.Mark);
            cmd.Parameters.AddWithValue("@model", car.Model);
            cmd.Parameters.AddWithValue("@color", car.Color);
            cmd.Parameters.AddWithValue("@year",  car.Year_of_release);
            cmd.Parameters.AddWithValue("@price", car.Price);
            cmd.Parameters.AddWithValue("@tech",  car.Technical_specifications);
            cmd.Parameters.AddWithValue("@photo", (object?)car.PhotoPath ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void DeleteCar(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var checkQuery = "SELECT COUNT(*) FROM Order_Car WHERE ID_Car=@id";
            using var checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@id", id);

            if ((int)checkCmd.ExecuteScalar() > 0)
                throw new InvalidOperationException("Нельзя удалить автомобиль, участвующий в заказах!");

            var query = "DELETE FROM Car WHERE ID_Car=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ============================
        // CRUD Клиенты
        // ============================

        public List<Client> GetAllClients()
        {
            var clients = new List<Client>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Добавлено ID_User в SELECT
            var query = "SELECT ID_Client, [Name], Surname, Middle_name, Passport_series, Passport_number, Phone_number, Email, ID_User FROM Client";
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                clients.Add(new Client
                {
                    ID_Client       = reader.GetInt32(0),
                    Name            = reader.GetString(1),
                    Surname         = reader.GetString(2),
                    Middle_name     = reader.IsDBNull(3)  ? null : reader.GetString(3),
                    Passport_series = reader.GetInt32(4),
                    Passport_number = reader.GetInt32(5).ToString(),
                    Phone_number    = reader.GetString(6),
                    Email           = reader.GetString(7),
                    ID_User         = reader.IsDBNull(8)  ? null : reader.GetInt32(8)
                });
            }
            return clients;
        }

        public void AddClient(Client client)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"INSERT INTO Client ([Name], Surname, Middle_name, Passport_series, Passport_number, Phone_number, Email)
                          VALUES (@name, @surname, @middle, @series, @number, @phone, @email)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name",    client.Name);
            cmd.Parameters.AddWithValue("@surname", client.Surname);
            cmd.Parameters.AddWithValue("@middle",  (object?)client.Middle_name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@series",  client.Passport_series);
            cmd.Parameters.AddWithValue("@number",  client.Passport_number);
            cmd.Parameters.AddWithValue("@phone",   client.Phone_number);
            cmd.Parameters.AddWithValue("@email",   client.Email);
            cmd.ExecuteNonQuery();
        }

        public void UpdateClient(Client client)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"UPDATE Client SET [Name]=@name, Surname=@surname, Middle_name=@middle,
                          Passport_series=@series, Passport_number=@number,
                          Phone_number=@phone, Email=@email
                          WHERE ID_Client=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id",      client.ID_Client);
            cmd.Parameters.AddWithValue("@name",    client.Name);
            cmd.Parameters.AddWithValue("@surname", client.Surname);
            cmd.Parameters.AddWithValue("@middle",  (object?)client.Middle_name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@series",  client.Passport_series);
            cmd.Parameters.AddWithValue("@number",  client.Passport_number);
            cmd.Parameters.AddWithValue("@phone",   client.Phone_number);
            cmd.Parameters.AddWithValue("@email",   client.Email);
            cmd.ExecuteNonQuery();
        }

        public void DeleteClient(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var checkQuery = "SELECT COUNT(*) FROM [Order] WHERE ID_Client=@id AND Order_status IN ('В обработке', 'Выполнен')";
            using var checkCmd = new SqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@id", id);

            if ((int)checkCmd.ExecuteScalar() > 0)
                throw new InvalidOperationException("Нельзя удалить клиента с активными заказами!");

            var deleteOrders = "DELETE FROM [Order] WHERE ID_Client=@id";
            using var delCmd = new SqlCommand(deleteOrders, conn);
            delCmd.Parameters.AddWithValue("@id", id);
            delCmd.ExecuteNonQuery();

            var query = "DELETE FROM Client WHERE ID_Client=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ============================
        // CRUD Сотрудники
        // ============================

        public List<Employee> GetAllEmployees()
        {
            var employees = new List<Employee>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Добавлено ID_User в SELECT
            var query = "SELECT ID_Employee, [Name], Surname, Middle_name, Business_phone_number, Post, ID_User FROM Employee";
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new Employee
                {
                    ID_Employee           = reader.GetInt32(0),
                    Name                  = reader.GetString(1),
                    Surname               = reader.GetString(2),
                    Middle_name           = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Business_phone_number = reader.GetString(4),
                    Post                  = reader.GetString(5),
                    ID_User               = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                });
            }
            return employees;
        }

        public void AddEmployee(Employee emp)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"INSERT INTO Employee ([Name], Surname, Middle_name, Business_phone_number, Post)
                          VALUES (@name, @surname, @middle, @phone, @post)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name",    emp.Name);
            cmd.Parameters.AddWithValue("@surname", emp.Surname);
            cmd.Parameters.AddWithValue("@middle",  (object?)emp.Middle_name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone",   emp.Business_phone_number);
            cmd.Parameters.AddWithValue("@post",    emp.Post);
            cmd.ExecuteNonQuery();
        }

        public void UpdateEmployee(Employee emp)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"UPDATE Employee SET [Name]=@name, Surname=@surname, Middle_name=@middle,
                          Business_phone_number=@phone, Post=@post
                          WHERE ID_Employee=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id",      emp.ID_Employee);
            cmd.Parameters.AddWithValue("@name",    emp.Name);
            cmd.Parameters.AddWithValue("@surname", emp.Surname);
            cmd.Parameters.AddWithValue("@middle",  (object?)emp.Middle_name ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@phone",   emp.Business_phone_number);
            cmd.Parameters.AddWithValue("@post",    emp.Post);
            cmd.ExecuteNonQuery();
        }

        public void DeleteEmployee(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var deleteLinks = "DELETE FROM Employee_Car WHERE ID_Employee=@id";
            using var delCmd = new SqlCommand(deleteLinks, conn);
            delCmd.Parameters.AddWithValue("@id", id);
            delCmd.ExecuteNonQuery();

            var query = "DELETE FROM Employee WHERE ID_Employee=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ============================
        // CRUD Дилеры
        // ============================

        public List<Diller> GetAllDillers()
        {
            var dillers = new List<Diller>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            // Добавлено ID_User в SELECT
            var query = "SELECT ID_Diller, Car_center_name, Phone_number, Email, ID_User FROM Diller";
            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                dillers.Add(new Diller
                {
                    ID_Diller       = reader.GetInt32(0),
                    Car_center_name = reader.GetString(1),
                    Phone_number    = reader.GetString(2),
                    Email           = reader.GetString(3),
                    ID_User         = reader.IsDBNull(4) ? null : reader.GetInt32(4)
                });
            }
            return dillers;
        }

        public void AddDiller(Diller diller)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"INSERT INTO Diller (Car_center_name, Phone_number, Email)
                          VALUES (@name, @phone, @email)";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name",  diller.Car_center_name);
            cmd.Parameters.AddWithValue("@phone", diller.Phone_number);
            cmd.Parameters.AddWithValue("@email", diller.Email);
            cmd.ExecuteNonQuery();
        }

        public void UpdateDiller(Diller diller)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"UPDATE Diller SET Car_center_name=@name, Phone_number=@phone, Email=@email
                          WHERE ID_Diller=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id",    diller.ID_Diller);
            cmd.Parameters.AddWithValue("@name",  diller.Car_center_name);
            cmd.Parameters.AddWithValue("@phone", diller.Phone_number);
            cmd.Parameters.AddWithValue("@email", diller.Email);
            cmd.ExecuteNonQuery();
        }

        public void DeleteDiller(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var deleteLinks = "DELETE FROM Diller_Car WHERE ID_Diller=@id";
            using var delCmd = new SqlCommand(deleteLinks, conn);
            delCmd.Parameters.AddWithValue("@id", id);
            delCmd.ExecuteNonQuery();

            var query = "DELETE FROM Diller WHERE ID_Diller=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ============================
        // CRUD Заказы
        // ============================

        public List<Order> GetAllOrders()
        {
            var orders = new List<Order>();
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"
                SELECT o.ID_Order, o.ID_Client, o.Order_status, o.Date_of_execution, o.Payment_method,
                       c.[Name] + ' ' + c.Surname AS ClientName,
                       car.Mark + ' ' + car.Model AS CarInfo
                FROM [Order] o
                JOIN Client c ON o.ID_Client = c.ID_Client
                LEFT JOIN Order_Car oc ON o.ID_Order = oc.ID_Order
                LEFT JOIN Car car      ON oc.ID_Car  = car.ID_Car";

            using var cmd = new SqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                orders.Add(new Order
                {
                    ID_Order          = reader.GetInt32(0),
                    ID_Client         = reader.GetInt32(1),
                    Order_status      = reader.GetString(2),
                    Date_of_execution = reader.GetDateTime(3),
                    Payment_method    = reader.GetString(4),
                    ClientName        = reader.IsDBNull(5) ? null : reader.GetString(5),
                    CarInfo           = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return orders;
        }

        public void AddOrder(Order order, int carId)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"INSERT INTO [Order] (ID_Client, Order_status, Date_of_execution, Payment_method)
                          VALUES (@client, @status, @date, @payment);
                          SELECT SCOPE_IDENTITY();";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@client",  order.ID_Client);
            cmd.Parameters.AddWithValue("@status",  order.Order_status);
            cmd.Parameters.AddWithValue("@date",    order.Date_of_execution);
            cmd.Parameters.AddWithValue("@payment", order.Payment_method);

            var orderId = Convert.ToInt32(cmd.ExecuteScalar());

            var linkQuery = "INSERT INTO Order_Car (ID_Order, ID_Car) VALUES (@order, @car)";
            using var linkCmd = new SqlCommand(linkQuery, conn);
            linkCmd.Parameters.AddWithValue("@order", orderId);
            linkCmd.Parameters.AddWithValue("@car",   carId);
            linkCmd.ExecuteNonQuery();
        }

        public void UpdateOrder(Order order)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var query = @"UPDATE [Order] SET ID_Client=@client, Order_status=@status,
                          Date_of_execution=@date, Payment_method=@payment
                          WHERE ID_Order=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id",      order.ID_Order);
            cmd.Parameters.AddWithValue("@client",  order.ID_Client);
            cmd.Parameters.AddWithValue("@status",  order.Order_status);
            cmd.Parameters.AddWithValue("@date",    order.Date_of_execution);
            cmd.Parameters.AddWithValue("@payment", order.Payment_method);
            cmd.ExecuteNonQuery();
        }

        public void DeleteOrder(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            var deleteLinks = "DELETE FROM Order_Car WHERE ID_Order=@id";
            using var delCmd = new SqlCommand(deleteLinks, conn);
            delCmd.Parameters.AddWithValue("@id", id);
            delCmd.ExecuteNonQuery();

            var query = "DELETE FROM [Order] WHERE ID_Order=@id";
            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        // ============================
        // Экспорт
        // ============================

        public void ExportCarsToExcel(string filePath)
        {
            var cars = GetAllCars();
            ExcelExportService.ExportCars(cars, filePath);
        }

        public void ExportOrdersToExcel(string filePath)
        {
            var orders = GetAllOrders();
            ExcelExportService.ExportOrders(orders, filePath);
        }

        // ============================
        // Назначение ролей
        // ============================

        // Назначить клиента сотрудником через хранимую процедуру
        public void AssignEmployeeRole(int idClient, string post)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("sp_AssignEmployeeRole", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ID_Client", idClient);
            cmd.Parameters.AddWithValue("@Post",      post);
            cmd.ExecuteNonQuery();
        }

        // Снять роль сотрудника — пользователь становится клиентом
        public void RemoveEmployeeRole(int idEmployee)
        {
            using var conn = new SqlConnection(_connectionString);
            conn.Open();

            using var cmd = new SqlCommand("sp_RemoveEmployeeRole", conn)
            {
                CommandType = System.Data.CommandType.StoredProcedure
            };
            cmd.Parameters.AddWithValue("@ID_Employee", idEmployee);
            cmd.ExecuteNonQuery();
        }
    }
}