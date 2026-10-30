using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class SaleResponseDetailDto
    {
        public int SaleDetailId { get; set; }
        public int ServiceId { get; set; }
        public string? ServiceName { get; set; }
        public int Quantity { get; set; }
        public string? Cost { get; set; }
    }
}
