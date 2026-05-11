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
        public static string? Login { get; set; }
        public static UserRole? Role { get; set; }
        public static bool IsAuthenticated => Login != null;
    }
}