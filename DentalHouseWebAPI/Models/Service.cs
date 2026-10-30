namespace DentalHouseWebAPI.Models
{
    public class Service
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal Cost { get; set; }
        public bool IsActive { get; set; }
    }
}
