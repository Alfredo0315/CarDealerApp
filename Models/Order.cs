namespace CarDealerApp.Models;

public class Order
{
    public int ID_Order { get; set; }
    public int ID_Client { get; set; }
    public string Order_status { get; set; } = string.Empty;
    public DateTime Date_of_execution { get; set; }
    public string Payment_method { get; set; } = string.Empty;

    // Навигационные свойства
    public string? ClientName { get; set; }
    public string? CarInfo { get; set; }
}