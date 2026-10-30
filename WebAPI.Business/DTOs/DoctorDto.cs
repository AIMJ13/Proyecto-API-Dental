using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class DoctorDto
    {
        public required int DoctorId { get; set; }
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public string? Phone { get; set; }
        public string? SpecialtyName { get; set; }
        public List<int> SpecialtyIds { get; set; } = new();
        public List<DoctorSpecialtyDto> Specialties { get; set; } = new();
        public bool IsActive { get; set; }
    }
}
