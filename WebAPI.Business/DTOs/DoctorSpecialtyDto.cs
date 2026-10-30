using System;
using System.Collections.Generic;

namespace WebAPI.Business.DTOs
{
    public class DoctorSpecialtyDto
    {
        public int SpecialtyId { get; set; }
        public string SpecialtyName { get; set; } = string.Empty;
    }
}
