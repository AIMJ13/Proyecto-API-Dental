using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Controllers
{
        [Route("api/[controller]")]
        [ApiController]
        [Authorize(Roles = "Administrador")]
    public class RolesController : ControllerBase
    {
            private readonly IRoleService _roleService;

            public RolesController(IRoleService roleService)
            {
                _roleService = roleService;
            }

            [HttpGet]
            public async Task<IActionResult> GetAll(
                [FromQuery] int pageNumber = 1,
                [FromQuery] int pageSize = 10)
            {
                var serviceResponse = await _roleService.GetAllAsync();

                if (serviceResponse.IsSuccess && serviceResponse.Data != null)
                {
                    var allRoles = serviceResponse.Data.Select(c => new RoleDto
                    {
                        Id = c.Id,
                        RoleName = c.RoleName
                    }).ToList();

                    var total = allRoles.Count;
                    var paged = allRoles.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                    return Ok(new
                    {
                        pageNumber,
                        pageSize,
                        totalRecords = total,
                        totalPages = (int)Math.Ceiling(total / (double)pageSize),
                        data = paged
                    });
                }

                var unsuccessfulResponse = new UnsuccessfulResponseDTO();

                switch (serviceResponse.MessageCode)
                {
                    case MessageCodes.NoData:
                        unsuccessfulResponse.Code = "200";
                        unsuccessfulResponse.Message = "No se encontraron registros";
                        unsuccessfulResponse.Details = new { info = " Temporalmente no hay registros en la BD" };

                        return Ok(unsuccessfulResponse);

                    default:
                        unsuccessfulResponse.Code = "500";
                        unsuccessfulResponse.Message = "Ocurrio un error inesperado";
                        unsuccessfulResponse.Details = new { info = "Error interno en la aplicacion" };

                        return StatusCode(500, unsuccessfulResponse);
                }
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

                var serviceResponse = await _roleService.GetByIdAsync(id);

                if (serviceResponse.IsSuccess)
                {
                    var roleDto = new RoleDto
                    {
                        Id = serviceResponse.Data!.Id,
                        RoleName = serviceResponse.Data!.RoleName,


                    };

                    return Ok(new { message = "Rol encontrado correctamente.", data = roleDto });
                }

                switch (serviceResponse.MessageCode)
                {
                    case MessageCodes.NotFound:
                        var unsuccessfulResponse = new UnsuccessfulResponseDTO()
                        {
                            Code = "404",
                            Message = "No se encontro  role asociado  al Id proporcionado",
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

                var ServiceResponse = await _roleService.GetByNameAsync(name);

                if (ServiceResponse.IsSuccess)
                {
                    var RoleDto = new RoleDto()
                    {
                        Id = ServiceResponse.Data!.Id,
                        RoleName = ServiceResponse.Data!.RoleName,
                    };

                    return Ok(new { message = "Rol encontrado correctamente.", data = RoleDto });
                }

                switch (ServiceResponse.MessageCode)
                {
                    case MessageCodes.NotFound:
                        unSuccessfulResponse.Code = "404";
                        unSuccessfulResponse.Message = ServiceResponse.Message ?? "No se encontro rol asociado";
                        unSuccessfulResponse.Details = new { Error = "No hay registro asociado al valor name proporcionado" };

                        return NotFound(unSuccessfulResponse);

                    default:
                        unSuccessfulResponse.Code = "500";
                        unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrio un error inesperado";

                        return StatusCode(500, unSuccessfulResponse);
                }
            }


            [HttpPost]
            public async Task<IActionResult> AddAsync([FromBody] CreateRoleDto roleDto)
            {

                var serviceResponse = await _roleService.CreateAsync(roleDto);

                if (serviceResponse.IsSuccess)
                {

                    var newRoleDto = new RoleDto
                    {
                        Id = serviceResponse.Data!.Id,
                        RoleName = serviceResponse.Data!.RoleName,

                    };


                    return CreatedAtAction(
                        nameof(GetById),
                        new { Id = newRoleDto.Id },
                        new { message = "Rol agregado correctamente.", data = newRoleDto }
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
                        unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del Rol" };
                        return Conflict(unSuccessfulResponse);

                    default:
                        unSuccessfulResponse.Code = "500";
                        unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                        unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                        return StatusCode(500, unSuccessfulResponse);



                }
            }


            [HttpPut("{id}")]
            public async Task<IActionResult> Update(int id, [FromBody] UpdateRoleDto dataRole)
            {


                var serviceResponse = await _roleService.UpdateAsync(id, dataRole);

                if (serviceResponse.IsSuccess)
                {
                    var updatedRole = new RoleDto
                    {

                        Id = serviceResponse.Data!.Id,
                        RoleName = serviceResponse.Data!.RoleName,


                    };

                    return Ok(new { message = "Rol actualizado correctamente.", data = updatedRole });
                }

                var unSuccessfulResponse = new UnsuccessfulResponseDTO();

                switch (serviceResponse.MessageCode)
                {
                    case MessageCodes.NotFound:
                        unSuccessfulResponse.Code = "404";
                        unSuccessfulResponse.Message = "No se encontró rol con el Id proporcionado";
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

                var serviceResponse = await _roleService.SetStateAsync(id, state);

                if (serviceResponse.IsSuccess && serviceResponse.Data != null)
                {

                    return Ok(new
                    {
                        Id = serviceResponse.Data.Id,
                        RoleName = serviceResponse.Data.RoleName,
                        State = serviceResponse.Data.State,
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



        }
    }

