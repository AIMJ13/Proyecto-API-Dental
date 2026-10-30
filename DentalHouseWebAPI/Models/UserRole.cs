using System.ComponentModel.DataAnnotations;
namespace DentalHouseWebAPI.Models
{
    public class UserRole
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
    }
}
