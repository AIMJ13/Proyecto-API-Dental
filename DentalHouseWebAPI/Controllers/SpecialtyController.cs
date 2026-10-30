using DentalHouseWebAPI.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador, Recepcionista, Doctor")]
    public class SpecialtyController : ControllerBase
    {
        private readonly ISpecialtyService _specialtyService;

        
        public SpecialtyController(ISpecialtyService specialtyService)
        {
            _specialtyService = specialtyService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var serviceResponse = await _specialtyService.GetPagedAsync(pageNumber, pageSize, searchTerm, isActive);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var specialtyDtoCollection = serviceResponse.Data.Data.Select(c => new SpecialtyDto
                {
                    SpecialtyId = c.SpecialtyId,
                    SpecialtyName = c.SpecialtyName,
                    State = c.State
                });

                var apiResponse = new
                {
                    pageNumber = serviceResponse.Data.PageNumber,
                    pageSize = serviceResponse.Data.PageSize,
                    totalRecords = serviceResponse.Data.TotalRecords,
                    totalPages = (int)Math.Ceiling((double)serviceResponse.Data.TotalRecords / serviceResponse.Data.PageSize),
                    data = specialtyDtoCollection,
                    Pagination = new
                    {
                        serviceResponse.Data.PageNumber,
                        serviceResponse.Data.PageSize,
                        serviceResponse.Data.TotalRecords,
                        TotalPages = (int)Math.Ceiling(
                            (double)serviceResponse.Data.TotalRecords /
                            serviceResponse.Data.PageSize)
                    }
                };

                return Ok(apiResponse);
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDTO();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unsuccessfulResponse.Code = "200";
                    unsuccessfulResponse.Message = "No se encontraron registros";
                    unsuccessfulResponse.Details = new
                    {
                        info = "Temporalmente no hay registros en la BD"
                    };

                    return Ok(unsuccessfulResponse);

                default:
                    unsuccessfulResponse.Code = "500";
                    unsuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unsuccessfulResponse.Details = new
                    {
                        info = "Error interno en la aplicación"
                    };

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
                return BadRequest(new UnsuccessfulResponseDTO
                {
                    Code = "400",
                    Message = "Id proporcionado debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                });
            }

            var serviceResponse = await _specialtyService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var specialtyDto = new SpecialtyDto
                {
                    SpecialtyId = serviceResponse.Data.SpecialtyId,
                    SpecialtyName = serviceResponse.Data.SpecialtyName,
                    State = serviceResponse.Data.State
                };

                return Ok(new { message = "Especialidad encontrada correctamente.", data = specialtyDto });
            }

            if (serviceResponse.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(new UnsuccessfulResponseDTO
                {
                    Code = "404",
                    Message = "No se encontró una Especialidad asociada al Id proporcionado",
                    Details = new { info = serviceResponse.Message }
                });
            }

            return StatusCode(500, new UnsuccessfulResponseDTO
            {
                Code = "500",
                Message = "Ocurrió un error",
                Details = new { info = serviceResponse.Message ?? "Error interno no esperado" }
            });
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

            var ServiceResponse = await _specialtyService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var SpecialtyDto = new SpecialtyDto()
                {
                    SpecialtyId = ServiceResponse.Data!.SpecialtyId,
                    SpecialtyName = ServiceResponse.Data.SpecialtyName,
                   
                };

                return Ok(SpecialtyDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "No se encontro Especialidad  asociada";
                    unSuccessfulResponse.Details = new { Error = "No hay registro asociado al valor name proporcionado" };

                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrio un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Administrador, Recepcionista")]
        public async Task<IActionResult> AddAsync([FromBody] CreateSpecialtyDto specialtyDto)
        {
            var serviceResponse = await _specialtyService.CreateAsync(specialtyDto);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var newSpecialtyDto = new SpecialtyDto
                {
                    SpecialtyId = serviceResponse.Data.SpecialtyId,
                    SpecialtyName = serviceResponse.Data.SpecialtyName,
                    State = serviceResponse.Data.State
                };

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = newSpecialtyDto.SpecialtyId },
                    new { message = "Especialidad agregada correctamente.", data = newSpecialtyDto }
                );
            }

            if (serviceResponse.MessageCode == MessageCodes.Conflict)
            {
                return Conflict(new UnsuccessfulResponseDTO
                {
                    Code = "409",
                    Message = "El registro no pudo guardarse por un conflicto",
                    Details = new { info = serviceResponse.Message }
                });
            }

            return StatusCode(500, new UnsuccessfulResponseDTO
            {
                Code = "500",
                Message = "Ocurrió un error inesperado",
                Details = new { info = serviceResponse.Message ?? "Error interno inesperado" }
            });
        }



        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateSpecialtyDto dataSpecialty)
        {
            var serviceResponse = await _specialtyService.UpdateAsync(id, dataSpecialty);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var updatedSpecialty = new SpecialtyDto
                {
                    SpecialtyId = serviceResponse.Data.SpecialtyId,
                    SpecialtyName = serviceResponse.Data.SpecialtyName,
                    State = serviceResponse.Data.State
                };

                return Ok(new { message = "Especialidad actualizada correctamente.", data = updatedSpecialty });
            }

            if (serviceResponse.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(new UnsuccessfulResponseDTO
                {
                    Code = "404",
                    Message = "No se encontró Especialidad con el Id proporcionado",
                    Details = new { info = serviceResponse.Message }
                });
            }

            if (serviceResponse.MessageCode == MessageCodes.Conflict)
            {
                return Conflict(new UnsuccessfulResponseDTO
                {
                    Code = "409",
                    Message = "El registro no pudo guardarse por un conflicto",
                    Details = new { info = serviceResponse.Message }
                });
            }

            return StatusCode(500, new UnsuccessfulResponseDTO
            {
                Code = "500",
                Message = "Ocurrió un error inesperado",
                Details = new { info = serviceResponse.Message ?? "Error interno inesperado" }
            });
        }


        [HttpPatch("{id}/state")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _specialtyService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                return Ok(new
                {
                    SpecialtyId = serviceResponse.Data.SpecialtyId,
                    SpecialtyName = serviceResponse.Data.SpecialtyName,
                    State = serviceResponse.Data.State,
                    Message = serviceResponse.Message
                });
            }

            if (serviceResponse.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(new UnsuccessfulResponseDTO
                {
                    Code = "404",
                    Message = serviceResponse.Message,
                    Details = new { info = "No se encontró la Especialidad" }
                });
            }

            return StatusCode(500, new UnsuccessfulResponseDTO
            {
                Code = "500",
                Message = serviceResponse.Message ?? "Error al cambiar el estado",
                Details = new { info = "Error al cambiar el estado" }
            });
        }

    }
}
