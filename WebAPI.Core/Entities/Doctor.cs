using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class DoctorSpecialtyInfo
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
    }

    public class Doctor
    {
        public int DoctorId { get; set; }
        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? SpecialtyName { get; set; }

        public List<int> SpecialtyIds { get; set; } = new();
        public List<DoctorSpecialtyInfo> Specialties { get; set; } = new();
        public bool State { get; set; }
        public bool IsActive { get; set; }
    }
}
