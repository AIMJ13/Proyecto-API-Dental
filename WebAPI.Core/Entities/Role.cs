using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class Role
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool State { get; set; }
    }
}
