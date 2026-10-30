using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class Sale
    {
        public int SaleId { get; set; }
        public int AppointmentId { get; set; }
        public string SaleDate { get; set; } = string.Empty;
        public int PatientId { get; set; }
        public int DoctorId { get; set; }
        public string? PatientName { get; set; }
        public string? DoctorName { get; set; }
        public decimal TotalAmount { get; set; }

        public int ServicesCount { get; set; }
        public IEnumerable<SaleDetail> Details { get; set; } = new List<SaleDetail>();



    }
}
