using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreateDoctorDto
    {
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre del Doctor debe tener entre 2 y 100 caracteres")]
        public required string FirstName { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido del Doctor debe tener entre 2 y 100 caracteres")]
        public required string LastName { get; set; }
        [StringLength(20, ErrorMessage = "El teléfono no debe exceder los 20 caracteres")]
        public string? Phone { get; set; }
        public List<int> SpecialtyIds { get; set; } = new();
        public int? SpecialtyId { get; set; }
        public bool State { get; set; }
    }
}
