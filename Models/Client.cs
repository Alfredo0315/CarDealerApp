namespace CarDealerApp.Models;

public class Client
{
    public int ID_Client { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Middle_name { get; set; }
    public int Passport_series { get; set; }
    public string Passport_number { get; set; } = string.Empty;
    public string Phone_number { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PasswordHash          { get; set; }
    public int? ID_User { get; set; }
    public string FullName => $"{Surname} {Name} {Middle_name}".Trim();
}