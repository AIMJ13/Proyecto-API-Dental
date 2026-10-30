using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Recepcionista, Doctor ,Administrador")]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _patientService;

        
        public PatientController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isActive = null)
        {
            var serviceResponse =
                await _patientService.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    searchTerm,
                    isActive);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var patientDtoCollection =
                    serviceResponse.Data.Data.Select(c => new PatientDto
                    {
                        PatientId = c.PatientId,
                        FirstName = c.FirstName,
                        LastName = c.LastName,
                        Phone = c.Phone,
                        Address = c.Address,
                        BirthDate = c.BirthDate,
                        IsActive = c.State
                    });

                return Ok(new
                {
                    pageNumber = serviceResponse.Data.PageNumber,
                    pageSize = serviceResponse.Data.PageSize,
                    totalRecords = serviceResponse.Data.TotalRecords,
                    totalPages = serviceResponse.Data.TotalPages,
                    data = patientDtoCollection
                });
            }

            var unsuccessfulResponse = new UnsuccessfulResponseDTO();

            unsuccessfulResponse.Code = "500";
            unsuccessfulResponse.Message = "Error inesperado";
            unsuccessfulResponse.Details = new
            {
                info = serviceResponse.Message
            };

            return StatusCode(500, unsuccessfulResponse);
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

            var serviceResponse = await _patientService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var PatientDto = new PatientDto()
                {
                    PatientId = serviceResponse.Data!.PatientId,
                    FirstName = serviceResponse.Data.FirstName,
                    LastName = serviceResponse.Data.LastName,
                    Phone = serviceResponse.Data.Phone,
                    Address = serviceResponse.Data.Address,
                    BirthDate = serviceResponse.Data.BirthDate,
                };

                return Ok(new
                {
                    message = "Paciente encontrado correctamente.",
                    data = PatientDto
                });
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDTO()
                    {
                        Code = "404",
                        Message = "No se encontro un Paciente asociado al Id proporcionado",
                        Details = new { info = serviceResponse.Message ?? "No se encontro el Id del Paciente solicitado" }
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
        [Authorize(Roles = "Recepcionista, Doctor, Administrador")]

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

            var ServiceResponse = await _patientService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var PatientDto = new PatientDto()
                {
                    PatientId = ServiceResponse.Data!.PatientId,
                    FirstName = ServiceResponse.Data.FirstName,
                    LastName = ServiceResponse.Data.LastName,
                    Phone = ServiceResponse.Data.Phone,
                    Address = ServiceResponse.Data.Address,
                    BirthDate = ServiceResponse.Data.BirthDate,
                    
                };

                return Ok(new
                {
                    message = "Paciente encontrado correctamente.",
                    data = PatientDto
                });
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "No se encontro a un Paciente asociado";
                    unSuccessfulResponse.Details = new { Error = "No hay registro asociado" };

                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrio un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpGet("byphone/{phone}")]
        [Authorize(Roles = "Recepcionista, Doctor, Administrador")]
        public async Task<IActionResult> GetByPhone(
            string phone,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(phone))
            {
                return BadRequest(new UnsuccessfulResponseDTO
                {
                    Code = "400",
                    Message = "El teléfono no puede estar vacío"
                });
            }

            var serviceResponse = await _patientService.GetByPhoneAsync(phone);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var dtos = serviceResponse.Data.Select(c => new PatientDto
                {
                    PatientId = c.PatientId,
                    FirstName = c.FirstName,
                    LastName = c.LastName,
                    Phone = c.Phone,
                    Address = c.Address,
                    BirthDate = c.BirthDate,
                    IsActive = c.State
                }).ToList();

                var total = dtos.Count;
                var paged = dtos.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                return Ok(new
                {
                    pageNumber,
                    pageSize,
                    totalRecords = total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    data = paged
                });
            }

            return NotFound(new UnsuccessfulResponseDTO
            {
                Code = "404",
                Message = "No se encontraron pacientes con ese teléfono"
            });
        }

        [HttpGet("active")]
        [Authorize(Roles = "Recepcionista, Doctor, Administrador")]
        public async Task<IActionResult> GetActive(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            return await GetAll(pageNumber, pageSize, isActive: true);
        }

        [HttpPost]
        [Authorize(Roles = "Recepcionista, Administrador")]
        public async Task<IActionResult> AddAsync([FromBody] CreatePatientDto PatientDto)
        {

            var serviceResponse = await _patientService.CreateAsync(PatientDto);

            if (serviceResponse.IsSuccess)
            {

                var newPatientDto = new PatientDto
                {
                    PatientId = serviceResponse.Data!.PatientId,
                    FirstName = serviceResponse.Data!.FirstName,
                    LastName = serviceResponse.Data!.LastName,
                    Phone = serviceResponse.Data!.Phone,
                    Address = serviceResponse.Data!.Address,
                    BirthDate = serviceResponse.Data!.BirthDate,
                    IsActive = serviceResponse.Data!.State
                };


                return CreatedAtAction(
                      nameof(GetById),
                      new { Id = newPatientDto.PatientId },
                      new
                      {
                      message = "Paciente agregado correctamente.",
                       data = newPatientDto
                      }
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
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del Paciente" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                    return StatusCode(500, unSuccessfulResponse);



            }
        }




        [HttpPut("{id}")]
        [Authorize(Roles = "Recepcionista, Administrador")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdatePatientDto dataPatient)
        {
            var serviceResponse = await _patientService.UpdateAsync(id, dataPatient);

            if (serviceResponse.IsSuccess)
            {

                var updatedPatient = new PatientDto
                {
                    PatientId = serviceResponse.Data!.PatientId,
                    FirstName = serviceResponse.Data!.FirstName,
                    LastName = serviceResponse.Data!.LastName,
                    Phone = serviceResponse.Data!.Phone,
                    Address = serviceResponse.Data!.Address,
                    BirthDate = serviceResponse.Data!.BirthDate,
                    IsActive = serviceResponse.Data!.State
                };

                return Ok(new
                {
                    message = "Paciente actualizado correctamente.",
                    data = updatedPatient
                });

            }

            var unSuccessfulResponse = new UnsuccessfulResponseDTO();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró al Paciente con el Id proporcionado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Paciente no encontrado" };
                    return StatusCode(404, unSuccessfulResponse);

                case MessageCodes.Conflict:
                    unSuccessfulResponse.Code = "409";
                    unSuccessfulResponse.Message = "El registro no pudo guardarse debido a un conflicto";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Hubo conflicto en la actualización del Paciente" };
                    return StatusCode(409, unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }


        [HttpPatch("{id}/state")]
        [Authorize(Roles = "Recepcionista, Administrador")]
        public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _patientService.SetStateAsync(id, state);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {

                return Ok(new
                {
                    PatientId = serviceResponse.Data.PatientId,
                    FirstName = serviceResponse.Data.FirstName,
                    LastName = serviceResponse.Data.LastName,
                    Phone = serviceResponse.Data.Phone,
                    Address = serviceResponse.Data.Address,
                    state = serviceResponse.Data.State,
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
