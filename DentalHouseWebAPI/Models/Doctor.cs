namespace DentalHouseWebAPI.Models
{
    public class Doctor
    {
        public int DoctorId { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public List<int> SpecialtyIds { get; set; } = new();
        public int? SpecialtyId { get; set; }
        public bool IsActive { get; set; }
    }
}
