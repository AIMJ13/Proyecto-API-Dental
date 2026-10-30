using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreateServiceDto
    {
        [Required]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre del Servicio debe tener entre 5 y  100 caracteres")]
        
        public required string ServiceName { get; set; }
        public int ServiceId { get; set; }
        [StringLength(500, ErrorMessage = "La descripcion no debe exceder los  500 caracteres")]
   
        public required string Cost { get; set; }
        public bool State { get; set; }
    }
}
