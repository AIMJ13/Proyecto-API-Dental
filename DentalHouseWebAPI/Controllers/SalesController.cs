using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DentalHouseWebAPI.Helpers;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador, Recepcionista")]
    public class SalesController : ControllerBase
    {
        private readonly ISaleService _saleService;
        private readonly ISystemLogService _logService;

        public SalesController(ISaleService saleService, ISystemLogService logService)
        {
            _saleService = saleService;
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? patientId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var response =
                await _saleService.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    patientId,
                    doctorId,
                    startDate,
                    endDate);

            if (response.IsSuccess && response.Data != null)
            {
                return Ok(new
                {
                    pageNumber = response.Data.PageNumber,
                    pageSize = response.Data.PageSize,
                    totalRecords = response.Data.TotalRecords,
                    totalPages = response.Data.TotalPages,
                    data = response.Data.Data
                });
            }

            return StatusCode(500, new
            {
                message = response.Message
            });
        }

        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetByPatient(
            int patientId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, patientId: patientId);
        }

        [HttpGet("bydoctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(
            int doctorId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, doctorId: doctorId);
        }

        [HttpGet("bydate")]
        public async Task<IActionResult> GetByDateRange(
            [FromQuery] DateTime startDate,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, startDate: startDate, endDate: endDate ?? startDate);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new UnsuccessfulResponseDTO
                {
                    Code = "400",
                    Message = "El Id debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                });

            var response = await _saleService.GetByIdAsync(id);

            if (response.IsSuccess)
                return Ok(new { message = "Venta encontrada correctamente.", data = response.Data });

            switch (response.MessageCode)
            {
                case MessageCodes.NotFound:
                    return NotFound(new UnsuccessfulResponseDTO
                    {
                        Code = "404",
                        Message = "No se encontró la venta con el Id proporcionado.",
                        Details = new { info = response.Message }
                    });
                default:
                    return StatusCode(500, new UnsuccessfulResponseDTO
                    {
                        Code = "500",
                        Message = "Error inesperado",
                        Details = new { info = response.Message }
                    });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Register([FromBody] CreateSaleDto dto)
        {
            var response = await _saleService.InsertAsync(dto);

            if (response.IsSuccess)
            {
                try
                {
                    var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
                    await _logService.LogAsync(new CreateSystemLogDto
                    {
                        UserId = userId,
                        UserName = userName ?? "Sistema",
                        UserRole = userRole,
                        Action = "REGISTER_SALE",
                        Module = "Sales",
                        EntityId = response.Data!.SaleId.ToString(),
                        Description = $"Venta #{response.Data.SaleId} registrada para cita #{response.Data.AppointmentId} (Paciente: '{response.Data.PatientName}').",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return CreatedAtAction(nameof(GetById),
                    new { id = response.Data!.SaleId },
                    new { message = "Venta registrada correctamente.", data = response.Data });
            }

            switch (response.MessageCode)
            {
                case MessageCodes.Conflict:
                    return Conflict(new UnsuccessfulResponseDTO
                    {
                        Code = "409",
                        Message = "Ya existe una venta registrada para esta cita.",
                        Details = new { info = response.Message }
                    });
                case MessageCodes.NotFound:
                    return NotFound(new UnsuccessfulResponseDTO
                    {
                        Code = "404",
                        Message = response.Message,
                        Details = new { info = "Verifica el Id de la cita" }
                    });
                case MessageCodes.ErrorValidation:
                    return BadRequest(new UnsuccessfulResponseDTO
                    {
                        Code = "400",
                        Message = "Error en la validación de datos",
                        Details = new { info = response.Message }
                    });
                default:
                    return StatusCode(500, new UnsuccessfulResponseDTO
                    {
                        Code = "500",
                        Message = "Error inesperado",
                        Details = new { info = response.Message }
                    });
            }
        }
    }
}