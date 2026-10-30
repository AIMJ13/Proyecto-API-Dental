using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class UpdateDoctorDto
    {
        [Required(ErrorMessage = "El nombre del Doctor es obligatorio")]
        public required int DoctorId { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }

        [StringLength(500, ErrorMessage = "El nombre no debe exceder los 100 caracteres")]
        public string? Phone { get; set; }
        public List<int> SpecialtyIds { get; set; } = new();
        public int? SpecialtyId { get; set; }

        [Required(ErrorMessage = "Especificar si el Doctor está activa o no")]
        public bool State { get; set; }
    }
}
