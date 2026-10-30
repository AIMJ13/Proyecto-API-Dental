using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class Service
    {
        public  int ServiceId { get; set; }
        public required string ServiceName { get; set; }
        public required string Cost { get; set; }
        public bool State { get; set; }
    }
}
