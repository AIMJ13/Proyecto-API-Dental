namespace DentalHouseWebAPI.Models
{
    public class Patient
    {
        public int PatientId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public DateTime? BirthDate { get; set; }
        public bool IsActive { get; set; }
    }
}
