namespace DentalHouseWebAPI.Models
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;


        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public bool State { get; set; }
    }
}
