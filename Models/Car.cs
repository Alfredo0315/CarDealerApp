namespace CarDealerApp.Models;

public class Car
{
    public int ID_Car { get; set; }
    public string Mark { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int Year_of_release { get; set; }
    public decimal Price { get; set; }
    public string Technical_specifications { get; set; } = string.Empty;
    public string? PhotoPath { get; set; }

    public override string ToString() => $"{Mark} {Model} ({Year_of_release})";
}