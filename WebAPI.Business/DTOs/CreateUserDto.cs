using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class CreateUserDto
    {
        [Required(ErrorMessage = "El username es obligatorio")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 5 y 100 caracteres")]
        public string UserName { get; set; } = string.Empty;
        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener entre 6 y 100 caracteres")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [StringLength(100, ErrorMessage = "El correo no debe exceder los 100 caracteres")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido")]
        public string Email { get; set; } = string.Empty;

        public required string Roles { get; set; }


    }
}
