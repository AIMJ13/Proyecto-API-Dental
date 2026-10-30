using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador, Recepcionista, Doctor")]
    public class DoctorController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        private static DoctorDto MapToDto(Doctor doctor)
        {
            return new DoctorDto
            {
                DoctorId = doctor.DoctorId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                Phone = doctor.Phone,
                SpecialtyName = doctor.SpecialtyName,
                SpecialtyIds = doctor.SpecialtyIds,
                Specialties = doctor.Specialties.Select(s => new DoctorSpecialtyDto
                {
                    SpecialtyId = s.SpecialtyId,
                    SpecialtyName = s.SpecialtyName
                }).ToList(),
                IsActive = doctor.IsActive
            };
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? specialtyId = null,
            [FromQuery] bool? isActive = null,
            [FromQuery] string? searchTerm = null)
        {
            var serviceResponse =
                await _doctorService.GetPagedAsync(pageNumber, pageSize, specialtyId, isActive, searchTerm);

            if (serviceResponse.IsSuccess)
            {
                var doctorDtoCollection = serviceResponse.Data.Doctors.Select(MapToDto);

                var response = new PagedResponse<DoctorDto>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = serviceResponse.Data.TotalRecords,
                    TotalPages = (int)Math.Ceiling(
                        serviceResponse.Data.TotalRecords / (double)pageSize),
                    Data = doctorDtoCollection
                };

                return Ok(response);
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
                    unsuccessfulResponse.Message = "Error inesperado";
                    unsuccessfulResponse.Details = new
                    {
                        info = "Error interno en la aplicación"
                    };

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

            var serviceResponse = await _doctorService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var doctorDto = MapToDto(serviceResponse.Data!);
                return Ok(doctorDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDTO()
                    {
                        Code = "404",
                        Message = "No se encontro un Doctor asociado al Id proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontro el Id del Doctor solicitado" }
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
                unSuccessfulResponse.Details = new { Error = "El nombre no puede ser nulo o vacio" };

                return BadRequest(unSuccessfulResponse);

            }

            var ServiceResponse = await _doctorService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var doctorDto = MapToDto(ServiceResponse.Data!);
                return Ok(doctorDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "No se encontro a un Doctor asociado";
                    unSuccessfulResponse.Details = new { Error = "No hay registro asociado" };

                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrio un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpGet("byspecialty/{specialtyId}")]
        public async Task<IActionResult> GetBySpecialty(
            int specialtyId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, specialtyId: specialtyId);
        }

        [HttpGet("active")]
        public async Task<IActionResult> GetActive(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            return await GetAll(pageNumber, pageSize, isActive: true);
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> AddAsync([FromBody] CreateDoctorDto DoctorDto)
        {

            var serviceResponse = await _doctorService.CreateAsync(DoctorDto);

            if (serviceResponse.IsSuccess)
            {
                var newDoctorDto = MapToDto(serviceResponse.Data!);

                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = newDoctorDto.DoctorId },
                    new { message = "Doctor agregado correctamente.", data = newDoctorDto }
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
                    unSuccessfulResponse.Message = "El registro no pudo guardarse debido un conflicto";
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del Doctor" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                    return StatusCode(500, unSuccessfulResponse);



            }
        }




        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateDoctorDto dataCategory)
        {
            var serviceResponse = await _doctorService.UpdateAsync(id, dataCategory);

            if (serviceResponse.IsSuccess)
            {
                var updatedDoctor = MapToDto(serviceResponse.Data!);

                return Ok(new { message = "Doctor actualizado correctamente.", data = updatedDoctor });
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDTO();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró al Doctor con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Doctor no encontrado" };
                    return StatusCode(404, unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse debido a un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización del Doctor" };
                    return StatusCode(409, unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }


        [HttpPatch("{id}/state")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _doctorService.SetStateAsync(id,state);

            if (serviceResponse.IsSuccess)
            {
                var doctorDto = MapToDto(serviceResponse.Data!);

                return Ok(new
                {
                    DoctorId = doctorDto.DoctorId,
                    FirstName = doctorDto.FirstName,
                    LastName = doctorDto.LastName,
                    Phone = doctorDto.Phone,
                    SpecialtyName = doctorDto.SpecialtyName,
                    SpecialtyIds = doctorDto.SpecialtyIds,
                    Specialties = doctorDto.Specialties,
                    state = serviceResponse.Data!.State,
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
