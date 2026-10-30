namespace DentalHouseWebAPI.Models
{
    public class Specialty
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}
