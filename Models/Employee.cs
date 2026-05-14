namespace CarDealerApp.Models;

public class Employee
{
    public int ID_Employee { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string? Middle_name { get; set; }
    public string Business_phone_number { get; set; } = string.Empty;
    public string Post { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? PasswordHash          { get; set; }
    public int? ID_User { get; set; }
    public string FullName => $"{Surname} {Name} {Middle_name}".Trim();
}