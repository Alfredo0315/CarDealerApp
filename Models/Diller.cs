namespace CarDealerApp.Models;

public class Diller
{
    public int ID_Diller { get; set; }
    public string Car_center_name { get; set; } = string.Empty;
    public string Phone_number { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int? ID_User { get; set; }
}