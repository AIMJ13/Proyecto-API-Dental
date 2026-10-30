using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using DentalHouseWebAPI.Helpers;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly ISystemLogService _logService;

        public AuthController(IAuthService authService, IUserService userService, ISystemLogService logService)
        {
            _authService = authService;
            _userService = userService;
            _logService = logService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterUserDto newUser)
        {
            var serviceResponse = await _authService.RegisterAsync(newUser);

            if (serviceResponse.IsSuccess)
            {

                var newUserDto = new UserDto
                {
                    Id = serviceResponse.Data!.Id,
                    UserName = serviceResponse.Data!.UserName,
                    Email = serviceResponse.Data!.Email,
                    State = serviceResponse.Data!.IsActive,
                };

                try
                {
                    var (_, _, _, ipAddress) = HttpContext.GetAuditUserInfo();
                    await _logService.LogAsync(new CreateSystemLogDto
                    {
                        UserId = newUserDto.Id,
                        UserName = newUserDto.UserName,
                        Action = "REGISTER",
                        Module = "Auth",
                        EntityId = newUserDto.Id.ToString(),
                        Description = $"Nuevo usuario '{newUserDto.UserName}' registrado en el sistema.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return CreatedAtAction(
                nameof(GetByUserName),
                new { username = newUserDto.UserName },
                new { message = "Usuario registrado correctamente.", data = newUserDto }
                 );
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDTO();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = "El dato proporcionado no es válido";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Revisa los campos enviados" };
                    return BadRequest(unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                    unSuccessfulResponse.Details = new { info = "El nombre del usuario o correo ya existe" };
                    return Conflict(unSuccessfulResponse);


                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return BadRequest(unSuccessfulResponse);
            }
        }

        [HttpGet("byusername/{username}")]
        public async Task<IActionResult> GetByUserName(string username)
        {
            var unSuccessfulResponse = new UnsuccessfulResponseDTO();
            if (username.IsNullOrEmpty())
            {
                unSuccessfulResponse.Code = "400";
                unSuccessfulResponse.Message = "El dato proporcionado no es valido";
                unSuccessfulResponse.Details = new { Error = "El name no puede ser nulo o vacio" };

                return BadRequest(unSuccessfulResponse);

            }

            var ServiceResponse = await _authService.GetByUserNameAsync(username);

            if (ServiceResponse.IsSuccess)
            {
                var UserDto = new UserDto()
                {
                    Id = ServiceResponse.Data!.Id,
                    UserName = ServiceResponse.Data.UserName,
                    Email = ServiceResponse.Data.Email,
                    State = ServiceResponse.Data.IsActive,

                };

                return Ok(new { message = "Usuario encontrado correctamente.", data = UserDto });
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "No se encontro usuario asociado";
                    unSuccessfulResponse.Details = new { Error = "No hay registro asociado al valor name proporcionado" };

                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrio un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }



        [HttpGet("byemail/{email}")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            var unSuccessfulResponse = new UnsuccessfulResponseDTO();


            if (email == null || email == "")
            {
                unSuccessfulResponse.Code = "400";
                unSuccessfulResponse.Message = "El dato proporcionado no es válido";
                unSuccessfulResponse.Details = new { Error = "El email no puede ser nulo o vacío" };
                return BadRequest(unSuccessfulResponse);
            }


            var serviceResponse = await _authService.GetByEmailAsync(email);

            if (serviceResponse.IsSuccess)
            {
                var userDto = new UserDto
                {
                    Id = serviceResponse.Data!.Id,
                    UserName = serviceResponse.Data.UserName,
                    Email = serviceResponse.Data.Email,
                    State = serviceResponse.Data.IsActive
                };

                return Ok(new { message = "Usuario encontrado correctamente.", data = userDto });
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "No se encontró usuario asociado";
                    unSuccessfulResponse.Details = new { Error = "No hay registro asociado al email proporcionado" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno " };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto loginRequest)
        {
            var serviceResponse = await _authService.LoginAsync(loginRequest);

            if (serviceResponse.IsSuccess)
            {
                try
                {
                    var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                    var roles = serviceResponse.Data?.Roles != null ? string.Join(", ", serviceResponse.Data.Roles) : null;
                    await _logService.LogAsync(new CreateSystemLogDto
                    {
                        UserId = serviceResponse.Data?.Id,
                        UserName = serviceResponse.Data?.UserName ?? loginRequest.UserName,
                        UserRole = roles,
                        Action = "LOGIN",
                        Module = "Auth",
                        EntityId = serviceResponse.Data?.Id.ToString(),
                        Description = $"Inicio de sesión exitoso para el usuario '{loginRequest.UserName}'.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return Ok(serviceResponse.Data);
            }

            try
            {
                var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
                await _logService.LogAsync(new CreateSystemLogDto
                {
                    UserName = loginRequest.UserName,
                    Action = "LOGIN_FAILED",
                    Module = "Auth",
                    Description = $"Intento fallido de inicio de sesión para el usuario '{loginRequest.UserName}'.",
                    IpAddress = ipAddress,
                    IsSuccess = false
                });
            }
            catch { }

            var unSuccessfulResponse = new UnsuccessfulResponseDTO();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Unauthorized:
                    unSuccessfulResponse.Code = "401";
                    unSuccessfulResponse.Message = "Error de autenticacion de usuario";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message };


                    return Unauthorized(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrio un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var (userId, _, _, _) = HttpContext.GetAuditUserInfo();
            if (userId == null || userId <= 0)
            {
                return Unauthorized(new UnsuccessfulResponseDTO
                {
                    Code = "401",
                    Message = "No se pudo identificar al usuario a partir del token proporcionado.",
                    Details = new { info = "Token inválido o expirado" }
                });
            }

            var serviceResponse = await _authService.GetProfileAsync(userId.Value);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    message = "Perfil obtenido correctamente.",
                    data = serviceResponse.Data
                });
            }

            if (serviceResponse.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(new UnsuccessfulResponseDTO
                {
                    Code = "404",
                    Message = "Usuario no encontrado.",
                    Details = new { info = serviceResponse.Message }
                });
            }

            return StatusCode(500, new UnsuccessfulResponseDTO
            {
                Code = "500",
                Message = serviceResponse.Message ?? "Error al obtener el perfil de usuario.",
                Details = new { info = "Error interno" }
            });
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
            if (userId == null || userId <= 0)
            {
                return Unauthorized(new UnsuccessfulResponseDTO
                {
                    Code = "401",
                    Message = "No se pudo identificar al usuario a partir del token proporcionado.",
                    Details = new { info = "Token inválido o expirado" }
                });
            }

            var serviceResponse = await _userService.ChangePasswordAsync(userId.Value, dto.CurrentPassword, dto.NewPassword);

            if (serviceResponse.IsSuccess)
            {
                try
                {
                    await _logService.LogAsync(new CreateSystemLogDto
                    {
                        UserId = userId,
                        UserName = userName ?? "Usuario",
                        UserRole = userRole,
                        Action = "CHANGE_PASSWORD",
                        Module = "Auth",
                        EntityId = userId.ToString(),
                        Description = $"El usuario '{userName}' cambió su contraseña exitosamente.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return Ok(new { message = "Contraseña actualizada exitosamente." });
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDTO();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Datos inválidos";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message };
                    return BadRequest(unSuccessfulResponse);

                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Usuario no encontrado";
                    unSuccessfulResponse.Details = new { info = "Usuario no encontrado" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }
    }
}
