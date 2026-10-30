using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DentalHouseWebAPI.Helpers;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador, Recepcionista, Doctor")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        private readonly ISystemLogService _logService;

        public AppointmentController(IAppointmentService appointmentService, ISystemLogService logService)
        {
            _appointmentService = appointmentService;
            _logService = logService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] int? doctorId = null,
            [FromQuery] int? patientId = null,
            [FromQuery] string? status = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null)
        {
            var response =
                await _appointmentService.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    doctorId,
                    patientId,
                    status,
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

        [HttpGet("bydoctor/{doctorId}")]
        public async Task<IActionResult> GetByDoctor(
            int doctorId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, doctorId: doctorId);
        }

        [HttpGet("bypatient/{patientId}")]
        public async Task<IActionResult> GetByPatient(
            int patientId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, patientId: patientId);
        }

        [HttpGet("bystatus/{status}")]
        public async Task<IActionResult> GetByStatus(
            string status,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            return await GetAll(pageNumber, pageSize, status: status);
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

        [HttpGet("today")]
        public async Task<IActionResult> GetToday(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var today = DateTime.Today;
            return await GetAll(pageNumber, pageSize, startDate: today, endDate: today);
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

            var response = await _appointmentService.GetByIdAsync(id);

            if (response.IsSuccess)
                return Ok(new { message = "Cita encontrada correctamente.", data = response.Data });

            switch (response.MessageCode)
            {
                case MessageCodes.NotFound:
                    return NotFound(new UnsuccessfulResponseDTO
                    {
                        Code = "404",
                        Message = "No se encontró la cita con el Id proporcionado.",
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
        [Authorize(Roles = "Administrador, Recepcionista")]
        public async Task<IActionResult> AddAsync([FromBody] CreateAppointmentDto dto)
        {
            var response = await _appointmentService.CreateAsync(dto);

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
                        Action = "CREATE_APPOINTMENT",
                        Module = "Appointments",
                        EntityId = response.Data!.AppointmentId.ToString(),
                        Description = $"Cita #{response.Data.AppointmentId} agendada para paciente '{response.Data.PatientName}' con Dr. '{response.Data.DoctorName}' el {response.Data.Date:yyyy-MM-dd} {response.Data.Time}.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return CreatedAtAction(nameof(GetById),
                    new { id = response.Data!.AppointmentId },
                    new { message = "Cita agendada correctamente.", data = response.Data });
            }

            switch (response.MessageCode)
            {
                case MessageCodes.Conflict:
                    return Conflict(new UnsuccessfulResponseDTO
                    {
                        Code = "409",
                        Message = response.Message,
                        Details = new { info = "Conflicto de horario: el doctor ya tiene una cita agendada en esa fecha y hora" }
                    });
                case MessageCodes.NotFound:
                    return NotFound(new UnsuccessfulResponseDTO
                    {
                        Code = "404",
                        Message = response.Message,
                        Details = new { info = "Verifica el Id del paciente o doctor" }
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

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador, Recepcionista")]
        public async Task<IActionResult> UpdateAsync(int id, [FromBody] UpdateAppointmentDto dto)
        {
            var response = await _appointmentService.UpdateAsync(id, dto);

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
                        Action = "UPDATE_APPOINTMENT",
                        Module = "Appointments",
                        EntityId = id.ToString(),
                        Description = $"Cita #{id} actualizada.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return Ok(new { message = "Cita actualizada correctamente.", data = response.Data });
            }

            switch (response.MessageCode)
            {
                case MessageCodes.NotFound:
                    return NotFound(new UnsuccessfulResponseDTO
                    {
                        Code = "404",
                        Message = "No se encontró la cita con el Id proporcionado.",
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

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromQuery] AppointmentStatusDto status)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    Code = "400",
                    Message = "El Id debe ser mayor a 0"
                });
            }

            string statusDb = status switch
            {
                AppointmentStatusDto.Programada => "Programada",
                AppointmentStatusDto.Completada => "Completada",
                AppointmentStatusDto.Cancelada => "Cancelada",
                AppointmentStatusDto.NoAsistio => "No asistió",
                _ => "Programada"
            };

            var serviceResponse = await _appointmentService.UpdateStatusAsync(id, statusDb);

            if (serviceResponse.MessageCode == MessageCodes.Success && serviceResponse.Data != null)
            {
                try
                {
                    var (userId, userName, userRole, ipAddress) = HttpContext.GetAuditUserInfo();
                    await _logService.LogAsync(new CreateSystemLogDto
                    {
                        UserId = userId,
                        UserName = userName ?? "Sistema",
                        UserRole = userRole,
                        Action = "UPDATE_STATUS",
                        Module = "Appointments",
                        EntityId = id.ToString(),
                        Description = $"Estado de la cita #{id} cambiado a '{statusDb}'.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                var appointmentDto = new AppointmentDto
                {
                    AppointmentId = serviceResponse.Data.AppointmentId,
                    Date = serviceResponse.Data.Date,
                    Time = serviceResponse.Data.Time,
                    Reason = serviceResponse.Data.Reason,
                    PatientId = serviceResponse.Data.PatientId,
                    DoctorId = serviceResponse.Data.DoctorId,
                    PatientName = serviceResponse.Data.PatientName,
                    DoctorName = serviceResponse.Data.DoctorName,
                    Status = serviceResponse.Data.Status
                };

                return Ok(new
                {
                    message = "Estado de cita actualizado correctamente.",
                    data = appointmentDto
                });
            }

            if (serviceResponse.MessageCode == MessageCodes.NotFound)
            {
                return NotFound(new
                {
                    Code = "404",
                    Message = "No se encontró la cita"
                });
            }

            return StatusCode(500, new
            {
                Code = "500",
                Message = "Error interno al actualizar el estado de la cita",
                Details = serviceResponse.Message
            });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            if (id <= 0)
                return BadRequest(new UnsuccessfulResponseDTO
                {
                    Code = "400",
                    Message = "El Id debe ser mayor a 0",
                    Details = new { info = "Error en el formato del valor enviado" }
                });

            var response = await _appointmentService.DeleteAsync(id);

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
                        Action = "DELETE_APPOINTMENT",
                        Module = "Appointments",
                        EntityId = id.ToString(),
                        Description = $"Cita #{id} eliminada del sistema.",
                        IpAddress = ipAddress,
                        IsSuccess = true
                    });
                }
                catch { }

                return Ok(new { message = "Cita eliminada correctamente." });
            }

            switch (response.MessageCode)
            {
                case MessageCodes.NotFound:
                    return NotFound(new UnsuccessfulResponseDTO
                    {
                        Code = "404",
                        Message = "No se encontró la cita con el Id proporcionado.",
                        Details = new { info = response.Message }
                    });

                case MessageCodes.Conflict:
                    return Conflict(new UnsuccessfulResponseDTO
                    {
                        Code = "409",
                        Message = "No se puede eliminar la cita porque tiene una venta registrada.",
                        Details = new { info = "Elimine primero la venta asociada" }
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