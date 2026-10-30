using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class SaleDto
    {
        public required int Id { get; set; }
        public required int DoctorId { get; set; }
        public int UserId { get; set; }
        public DateTime DateTime { get; set; }
        public Decimal Total { get; set; }
    }
}
