using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class UpdateServiceDto
    {
        [Required(ErrorMessage = "El Id del Servicio es obligatorio")]
        public required int ServiceId { get; set; }
        [Required(ErrorMessage = "El nombre del Servicio es obligatorio")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "El nombre del Servicio debe tener entre 5 y 100 caracteres")]
        public required string ServiceName { get; set; }
        public required string Cost { get; set; }
        public bool State { get; set; }
    }
}
