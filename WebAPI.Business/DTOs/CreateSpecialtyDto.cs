using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreateSpecialtyDto
    {
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre de la Especialidad debe tener entre 5 y  100 caracteres")]
        public required string SpecialtyName { get; set; }

        public bool State { get; set; }
    }
}
