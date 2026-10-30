using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreateSaleDetailDto
    {
        [Required(ErrorMessage = "El Id del servicio es obligatorio")]
        [Range(1, int.MaxValue, ErrorMessage = "El Id del servicio no es válido")]
        public int ServiceId { get; set; }
        [Required(ErrorMessage = "La cantidad es obligatoria")]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a 0")]
        public int Quantity { get; set; }
    }
}
