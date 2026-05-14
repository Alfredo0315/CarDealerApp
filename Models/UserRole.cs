namespace CarDealerApp.Models
{
    public enum UserRole
    {
        Admin,
        Employee,
        Client,
        Diller
    }

    public static class CurrentUser
    {
        public static string? Email { get; set; } 
        public static string? Login  
        {
            get => Email;
            set => Email = value;
        }
        public static UserRole? Role { get; set; }
        public static int ClientId { get; set; }    
        public static int EmployeeId { get; set; } 
        public static bool IsAuthenticated => Email != null;
    }
}