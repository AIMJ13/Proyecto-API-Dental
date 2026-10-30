using System.ComponentModel.DataAnnotations;

namespace DentalHouseWebAPI.Models
{
    public class Roles
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string admin { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;
    }
}