using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class Specialty
    {
        public int SpecialtyId { get; set; }
        public required string SpecialtyName { get; set; }
        public bool State { get; set; }
    }
}
