namespace DentalHouseWebAPI.Models
{
    public class Sale
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public Decimal Total { get; set; }
    }
}
