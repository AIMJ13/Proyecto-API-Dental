using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class RegisterUserDto
    {
        [Required, MinLength(5)]
        public string UserName { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required, MinLength(5)]
        public string Password { get; set; } = string.Empty;
        [Compare("Password", ErrorMessage = "LAS CONTRASEÑAS NO COINCIDEN")]
        public string ConfirmPassword { get; set; } = string.Empty;

        public IEnumerable<string> Roles { get; set; } = Enumerable.Empty<string>();
    }
}
