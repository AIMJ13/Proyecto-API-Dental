using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class ServiceUpdateDto
    {
        public required string serviceId {  get; set; }
        public required string serviceName { get; set; }
        public required string Cost {  get; set; }
    }
}
