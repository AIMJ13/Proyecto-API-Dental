using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreateSaleDto
    {
        [Required(ErrorMessage = "El Id de la cita es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El Id de la cita no es válido")]
        public int AppointmentId { get; set; }
        [Required(ErrorMessage = "La venta debe incluir al menos un servicio")]
        [MinLength(1, ErrorMessage = "El detalle debe incluir al menos un servicio")]
        public List<CreateSaleDetailDto> Details { get; set; } = new();
    }
}
