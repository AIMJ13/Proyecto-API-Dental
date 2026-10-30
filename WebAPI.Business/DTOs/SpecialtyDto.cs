using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class SpecialtyDto
    {
        public int? SpecialtyId { get; set; }
        public string? SpecialtyName { get; set; }

        public bool State { get; set; }
    }
}
