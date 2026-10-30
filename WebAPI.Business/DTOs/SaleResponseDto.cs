using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class SaleResponseDto
    {
        public int SaleId { get; set; }
        public int AppointmentId { get; set; }
        public string? SaleDate { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public List<SaleResponseDetailDto>? Details { get; set; }
    }
}
