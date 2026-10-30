using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreateAppointmentDto
    {
        public required string Date { get; set; }
        public required string Time { get; set; }
        public string? Reason { get; set; }
        public required int PatientId { get; set; }
        public required int DoctorId { get; set; }
    }
}
