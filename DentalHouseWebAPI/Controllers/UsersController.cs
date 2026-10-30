using Microsoft.AspNetCore.Authorization;
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
    [Authorize(Roles = "Administrador")]  
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ISystemLogService _logService;

        public UsersController(IUserService userService, ISystemLogService logService)
        {
            _userService = userService;
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var serviceResponse = await _userService.GetPagedAsync(pageNumber, pageSize, searchTerm, isActive);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var usersDtoCollection = serviceResponse.Data.Data.Select(c => new UserDto
                {
                    Id = c.Id,
                    UserName = c.UserName,
                    Email = c.Email,
                    State = c.IsActive,
                    Roles = c.Roles
                });

                return Ok(new
                {
                    pageNumber = serviceResponse.Data.PageNumber,
                    pageSize = serviceResponse.Data.PageSize,
                    totalRecords = serviceResponse.Data.TotalRecords,
                    totalPages = serviceResponse.Data.TotalPages,
                    data = usersDtoCollection
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDTO();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros";
                    unsuccessfulResponse.Details = new { info = "Temporalmente no hay registros en la BD" };

                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unsuccessfulResponse.Details = new { info = "Error interno en la aplicacion" };

                    return StatusCode(500, unsuccessfulResponse);
            }
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            return await GetAll(pageNumber, pageSize, isActive: true);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
            {
                var response = new UnsuccessfulResponseDTO()
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }

                };

                return BadRequest(response);
            }

            var serviceResponse = await _userService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var userDto = new UserDto
                {
                    Id = serviceResponse.Data!.Id,
                    UserName = serviceResponse.Data!.UserName,
                    Email = serviceResponse.Data!.Email,
                    State = serviceResponse.Data!.IsActive,
                    Roles = serviceResponse.Data!.Roles
                };

                return Ok(new { message = "Usuario encontrado correctamente.", data = userDto });
            }

                switch (serviceResponse.MessageCode)
                {
                    case MessageCodes.NotFound:
                        var unsuccessfulResponse = new UnsuccessfulResponseDTO()
                        {
                            Code = "404",
                            Message = "No se encontro usuario asociada al Id proporcionado",
                            Details = new { info = serviceResponse.Message ?? "No se encontro el recurso solicitado" }
                        };

                        return NotFound(unsuccessfulResponse);

                    default:
                        unsuccessfulResponse = new UnsuccessfulResponseDTO()
                        {
                            Code = "500",
                            Message = "Ocurrio un error",
                            Details = new { info = serviceResponse.Message ?? "Error interno no esperado" }
                        };

                        return StatusCode(500, unsuccessfulResponse);
                }
            }

            [HttpGet("byname/{name}")]
            public async Task<IActionResult> GetByName(string name)
            {
                var unSuccessfulResponse = new UnsuccessfulResponseDTO();
                if (name.IsNullOrEmpty())
                {
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = "El dato proporcionado no es valido";
                    unSuccessfulResponse.Details = new { Error = "El name no puede ser nulo o vacio" };

                    return BadRequest(unSuccessfulResponse);

                }

                var ServiceResponse = await _userService.GetByNameAsync(name);

                if (ServiceResponse.IsSuccess)
                {
                    var UserDto = new UserDto()
                    {
                        Id = ServiceResponse.Data!.Id,
                        UserName = ServiceResponse.Data.UserName,
                        Email = ServiceResponse.Data.Email,
                        State = ServiceResponse.Data.IsActive,
                        Roles = ServiceResponse.Data.Roles,
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


            [HttpPost]
            public async Task<IActionResult> AddAsync([FromBody] CreateUserDto userDto)
            {

                var serviceResponse = await _userService.CreateAsync(userDto);

                if (serviceResponse.IsSuccess)
                {

                    var newUserDto = new UserDto
                    {
                        Id = serviceResponse.Data!.Id,
                        UserName = serviceResponse.Data!.UserName,
                        Email = serviceResponse.Data!.Email,
                        State = serviceResponse.Data!.IsActive,
                        Roles = serviceResponse.Data!.Roles

                    };

                    try
                    {
                        var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
                        await _logService.LogAsync(new CreateSystemLogDto
                        {
                            UserId = userId,
                            UserName = userName ?? newUserDto.UserName,
                            UserRole = userRole,
                            Action = "CREATE_USER",
                            Module = "Users",
                            EntityId = newUserDto.Id.ToString(),
                            Description = $"Usuario '{newUserDto.UserName}' creado con roles '{string.Join(", ", newUserDto.Roles ?? new List<string>())}'.",
                            IpAddress = ipAddress,
                            IsSuccess = true
                        });
                    }
                    catch { }

                    return CreatedAtAction(
                        nameof(GetById),
                        new { Id = newUserDto.Id },
                        new { message = "Usuario agregado correctamente.", data = newUserDto }
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
                        unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del usuario" };
                        return Conflict(unSuccessfulResponse);

                    default:
                        unSuccessfulResponse.Code = "500";
                        unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                        unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                        return StatusCode(500, unSuccessfulResponse);



                }
            }



            [HttpPut("{id}")]
            public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dataUser)
            {


                var serviceResponse = await _userService.UpdateAsync(id, dataUser);

                if (serviceResponse.IsSuccess)
                {

                    var updatedUser = new UserDto
                    {
                        Id = serviceResponse.Data!.Id,
                        UserName = serviceResponse.Data!.UserName,
                        Email = serviceResponse.Data!.Email,
                        State = serviceResponse.Data!.IsActive,
                        Roles = serviceResponse.Data!.Roles
                    };

                    try
                    {
                        var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
                        await _logService.LogAsync(new CreateSystemLogDto
                        {
                            UserId = userId,
                            UserName = userName ?? updatedUser.UserName,
                            UserRole = userRole,
                            Action = "UPDATE_USER",
                            Module = "Users",
                            EntityId = id.ToString(),
                            Description = $"Usuario '{updatedUser.UserName}' (ID #{id}) actualizado.",
                            IpAddress = ipAddress,
                            IsSuccess = true
                        });
                    }
                    catch { }

                    return Ok(new { message = "Usuario actualizado correctamente.", data = updatedUser });
            }

                var unSuccessfulResponse = new UnsuccessfulResponseDTO();

                switch (serviceResponse.MessageCode)
                {
                    case MessageCodes.NotFound:
                        unSuccessfulResponse.Code = "404";
                        unSuccessfulResponse.Message = "No se encontró usuario con el Id proporcionado";
                        unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Recurso no encontrado" };
                        return StatusCode(404, unSuccessfulResponse);

                    case MessageCodes.Conflict:
                        unSuccessfulResponse.Code = "409";
                        unSuccessfulResponse.Message = "El registro no pudo guardarse por un conflicto";
                        unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización" };
                        return StatusCode(409, unSuccessfulResponse);

                    default:
                        unSuccessfulResponse.Code = "500";
                        unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                        unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                        return StatusCode(500, unSuccessfulResponse);
                }
            }




            [HttpPatch("{id}/state")]
            public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
            {
                var serviceResponse = await _userService.SetStateAsync(id, state);

                if (serviceResponse.IsSuccess)
                {
                    try
                    {
                        var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
                        await _logService.LogAsync(new CreateSystemLogDto
                        {
                            UserId = userId,
                            UserName = userName ?? "Sistema",
                            UserRole = userRole,
                            Action = "UPDATE_USER_STATE",
                            Module = "Users",
                            EntityId = id.ToString(),
                            Description = $"Estado del usuario '{serviceResponse.Data!.UserName}' (ID #{id}) cambiado a {(state ? "Activo" : "Inactivo")}.",
                            IpAddress = ipAddress,
                            IsSuccess = true
                        });
                    }
                    catch { }

                    return Ok(new
                    {
                        Id = serviceResponse.Data!.Id,
                        UserName = serviceResponse.Data.UserName,
                        Email = serviceResponse.Data.Email,
                        State = serviceResponse.Data.IsActive,
                        Message = serviceResponse.Message
                    });
                }

                var unSuccessfulResponse = new UnsuccessfulResponseDTO
                {
                    Code = serviceResponse.MessageCode == MessageCodes.ErrorValidation ? "400" : "500",
                    Message = serviceResponse.Message,
                    Details = new { info = "Error al cambiar el estado" }
                };

                return BadRequest(unSuccessfulResponse);
            }

            [HttpDelete("{id}")]
            public async Task<IActionResult> Delete(int id)
            {
                if (id <= 0)
                {
                    return BadRequest(new UnsuccessfulResponseDTO
                    {
                        Code = "400",
                        Message = "Id proporcionado debe ser mayor a 0",
                        Details = new { info = "Error en el formato del valor enviado" }
                    });
                }

                var serviceResponse = await _userService.SetStateAsync(id, false);

                if (serviceResponse.IsSuccess)
                {
                    try
                    {
                        var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
                        await _logService.LogAsync(new CreateSystemLogDto
                        {
                            UserId = userId,
                            UserName = userName ?? "Sistema",
                            UserRole = userRole,
                            Action = "DELETE_USER",
                            Module = "Users",
                            EntityId = id.ToString(),
                            Description = $"Usuario '{serviceResponse.Data!.UserName}' (ID #{id}) eliminado lógicamente.",
                            IpAddress = ipAddress,
                            IsSuccess = true
                        });
                    }
                    catch { }

                    return Ok(new
                    {
                        message = "Usuario eliminado correctamente.",
                        data = new
                        {
                            Id = serviceResponse.Data!.Id,
                            UserName = serviceResponse.Data.UserName,
                            Email = serviceResponse.Data.Email,
                            State = serviceResponse.Data.IsActive
                        }
                    });
                }

                var unSuccessfulResponse = new UnsuccessfulResponseDTO
                {
                    Code = serviceResponse.MessageCode == MessageCodes.NotFound ? "404" : "500",
                    Message = serviceResponse.Message ?? "Error al eliminar el usuario.",
                    Details = new { info = "No se pudo completar la eliminación del usuario." }
                };

                return StatusCode(serviceResponse.MessageCode == MessageCodes.NotFound ? 404 : 500, unSuccessfulResponse);
            }





            [HttpPost("assign-role")]
            public async Task<IActionResult> AssignUserRoleAsync([FromBody] UserRoleDto userRole)
            {
                var serviceResponse = await _userService.AssignUserRoleAsync(userRole.UserId, userRole.RoleId);

                if (serviceResponse.IsSuccess)
                {
                    try
                    {
                        var (userId, userName, userRoleName, ipAddress) = HttpContext.GetAuditUserInfo();
                        await _logService.LogAsync(new CreateSystemLogDto
                        {
                            UserId = userId,
                            UserName = userName ?? "Sistema",
                            UserRole = userRoleName,
                            Action = "ASSIGN_ROLE",
                            Module = "Users",
                            EntityId = userRole.UserId.ToString(),
                            Description = $"Rol ID #{userRole.RoleId} asignado al usuario ID #{userRole.UserId}.",
                            IpAddress = ipAddress,
                            IsSuccess = true
                        });
                    }
                    catch { }

                    var userRoleDto = new UserRoleDto
                    {
                        UserId = serviceResponse.Data!.UserId,
                        RoleId = serviceResponse.Data.RoleId
                    };


                    return Ok(new { message = "Rol asignado correctamente.", data = userRoleDto });
                }

                var unSuccessfulResponse = new UnsuccessfulResponseDTO();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "El rol ya está asignado a este usuario";
                    unSuccessfulResponse.Details = new { info = "El rol ya existe para este usuario" };
                    return Conflict(unSuccessfulResponse);

                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "No se encontró el usuario o rol";
                    unSuccessfulResponse.Details = new { info = "Revise el UserId y RoleId proporcionado" };
                    return NotFound(unSuccessfulResponse);

                case MessageCodes.ErrorValidation:
                    unSuccessfulResponse.Code = "400";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Los datos proporcionados no son válidos";
                    unSuccessfulResponse.Details = new { info = "Revisa los campos proporcionados" };
                    return BadRequest(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno de la aplicación" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpPost("{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordDto dto)
        {
            if (id <= 0)
            {
                return BadRequest(new UnsuccessfulResponseDTO
                {
                    Code = "400",
                    Message = "El Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                });
            }

            var serviceResponse = await _userService.ResetPasswordAsync(id, dto.NewPassword);

            if (serviceResponse.IsSuccess)
            {
                try
                {
                    var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
                    await _logService.LogAsync(new CreateSystemLogDto
                    {
                        UserId = userId,
                        UserName = userName ?? "Sistema",
                        UserRole = userRole,
                        Action = "RESET_PASSWORD",
                        Module = "Users",
                        EntityId = id.ToString(),
                        Description = $"Contraseña del usuario ID #{id} restablecida por el administrador.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return Ok(new { message = "Contraseña restablecida correctamente." });
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDTO();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Usuario no encontrado";
                    unSuccessfulResponse.Details = new { info = "No se encontró el usuario con el Id proporcionado" };
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = serviceResponse.Message ?? "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpPost("{id}/sync-roles")]
        public async Task<IActionResult> SyncRoles(int id, [FromBody] string roles)
        {
            var serviceResponse = await _userService.SyncUserRolesAsync(id, roles);
            if (serviceResponse.IsSuccess)
            {
                return Ok(new { message = "Roles actualizados correctamente." });
            }
            return StatusCode(500, new UnsuccessfulResponseDTO { Code = "500", Message = serviceResponse.Message });
        }






        }

    }

