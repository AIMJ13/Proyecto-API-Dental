using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? PasswordHash { get; set; }
        public string? Email { get; set; }
        public bool IsActive { get; set; }
        public List<string> Roles { get; set; } = new();
    }
}