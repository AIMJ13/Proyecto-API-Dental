using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class ServiceCreateDto
    {
        public required string ServiceName { get; set; }
        public required string Cost {  get; set; }
    }
}
