using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace WebAPI.Business.DTOs
{
    public class AppointmentDto
    {
        public int AppointmentId { get; set; }

        public string Date { get; set; } = string.Empty;

        public string Time { get; set; } = string.Empty;

        public string? Reason { get; set; }

        public int PatientId { get; set; }

        public int DoctorId { get; set; }

        public string? PatientName { get; set; }

        public string? DoctorName { get; set; }

        public string Status { get; set; } = "Programada";
    }
}
