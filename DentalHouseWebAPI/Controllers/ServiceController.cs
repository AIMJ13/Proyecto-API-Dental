using DentalHouseWebAPI.Models;
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
    [Authorize(Roles = "Doctor, Recepcionista, Administrador")]
    public class ServiceController : ControllerBase
    {
        private readonly IServiceService _serviceService;

       
        public ServiceController(IServiceService serviceService)
        {
            _serviceService = serviceService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? searchTerm = null,
            [FromQuery] decimal? minPrice = null,
            [FromQuery] decimal? maxPrice = null,
            [FromQuery] bool? isActive = null)
        {
            var serviceResponse = await _serviceService.GetPagedAsync(pageNumber, pageSize, searchTerm, minPrice, maxPrice, isActive);

            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var serviceDtoCollection = serviceResponse.Data.Data.Select(c => new ServiceDto
                {
                    ServiceId = c.ServiceId,
                    ServiceName = c.ServiceName,
                    Cost = c.Cost,
                    State = c.State
                });

                return Ok(new
                {
                    pageNumber = serviceResponse.Data.PageNumber,
                    pageSize = serviceResponse.Data.PageSize,
                    totalRecords = serviceResponse.Data.TotalRecords,
                    totalPages = serviceResponse.Data.TotalPages,
                    data = serviceDtoCollection
                });
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

            var serviceResponse = await _serviceService.GetByIdAsync(id);

            if (serviceResponse.IsSuccess)
            {
                var serviceDto = new ServiceDto
                {
                    ServiceId = serviceResponse.Data!.ServiceId,
                    ServiceName = serviceResponse.Data!.ServiceName,
                    Cost = serviceResponse.Data!.Cost,
                    State = serviceResponse.Data!.State
                };

                return Ok(serviceDto);
            }

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    var unsuccessfulResponse = new UnsuccessfulResponseDTO()
                    {
                        Code = "404",
                        Message = "No se encontro un servicio asociada al Id proporcionado",
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

            var ServiceResponse = await _serviceService.GetByNameAsync(name);

            if (ServiceResponse.IsSuccess)
            {
                var ServiceDto = new ServiceDto()
                {
                    ServiceId = ServiceResponse.Data!.ServiceId,
                    ServiceName = ServiceResponse.Data.ServiceName,
                    Cost = ServiceResponse.Data.Cost,
                    State = ServiceResponse.Data!.State
                };

                return Ok(ServiceDto);
            }

            switch (ServiceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "No se encontro servicio  asociada";
                    unSuccessfulResponse.Details = new { Error = "No hay registro asociado al valor name proporcionado" };

                    
                    return NotFound(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = ServiceResponse.Message ?? "Ocurrio un error inesperado";

                    return StatusCode(500, unSuccessfulResponse);
            }
        }

        [HttpGet("bypricerange")]
        public async Task<IActionResult> GetByPriceRange(
            [FromQuery] decimal minPrice,
            [FromQuery] decimal maxPrice,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, minPrice: minPrice, maxPrice: maxPrice);
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
        public async Task<IActionResult> AddAsync([FromBody] CreateServiceDto serviceDto)
        {

            var serviceResponse = await _serviceService.CreateAsync(serviceDto);

            if (serviceResponse.IsSuccess)
            {

                var newServiceDto = new ServiceDto
                {
                    ServiceId = serviceResponse.Data!.ServiceId,
                    ServiceName = serviceResponse.Data!.ServiceName,
                    Cost = serviceResponse.Data!.Cost,
                    State = serviceResponse.Data!.State
                };


                return CreatedAtAction(
                    nameof(GetById),
                    new { Id = newServiceDto.ServiceId },
                   new { message = "Servicio agregado correctamente.", data = newServiceDto }
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
                    unSuccessfulResponse.Details = new { info = "No se puede duplicar el nombre del servicio" };
                    return Conflict(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrió un error inesperado";
                    unSuccessfulResponse.Details = new { info = serviceResponse.Message ?? "Error interno inesperado" };

                    return StatusCode(500, unSuccessfulResponse);



            }
        }




        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dataService)
        {
            if (id != dataService.ServiceId)
            {
                var response = new UnsuccessfulResponseDTO()
                {
                    Code = "400",
                    Message = "El Id de la ruta no coincide con el Id del cuerpo de la solicitud",
                    Details = new { info = "Error en el formato del valor enviado" }
                };
                return BadRequest(response);
            }

            var serviceResponse = await _serviceService.UpdateAsync(dataService);

            if (serviceResponse.IsSuccess)
            {
                var updatedService = new ServiceDto
                {
                    ServiceId = serviceResponse.Data!.ServiceId,
                    ServiceName = serviceResponse.Data!.ServiceName,
                    Cost = serviceResponse.Data!.Cost,
                    State = serviceResponse.Data!.State

                };

                return Ok(new { message = "Servicio actualizado correctamente.", data = updatedService });
            }

            var unSuccessfulResponse = new UnsuccessfulResponseDTO();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NotFound:
                    unSuccessfulResponse.Code = "404";
                    unSuccessfulResponse.Message = "No se encontró un servicio con el Id proporcionado";
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
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> SetStateAsync(int id, [FromQuery] bool state)
        {
            var serviceResponse = await _serviceService.SetStateAsync(id,state);

            if (serviceResponse.IsSuccess)
            {
                
                return Ok(new
                {
                    ServiceId = serviceResponse.Data!.ServiceId,
                    ServiceName = serviceResponse.Data.ServiceName,
                    Cost = serviceResponse.Data.Cost,
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
