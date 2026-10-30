using System;
using System.Threading.Tasks;
using DentalHouseWebAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("business")]
        public async Task<IActionResult> GetBusinessKpis([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetBusinessKpisAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar los KPIs de negocio.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("business/summary")]
        public async Task<IActionResult> GetSummary([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetSummaryAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar el resumen de negocio.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("business/doctors")]
        public async Task<IActionResult> GetTopDoctors([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetTopDoctorsAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar el top de doctores.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("business/services")]
        public async Task<IActionResult> GetTopServices([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetTopServicesAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar los servicios mas rentables.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("business/specialties")]
        public async Task<IActionResult> GetSpecialties([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetSpecialtiesAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar la participacion por especialidad.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("business/recurrence")]
        public async Task<IActionResult> GetPatientRecurrence([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetPatientRecurrenceAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar la recurrencia de pacientes.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("business/monthly")]
        public async Task<IActionResult> GetMonthlyRevenue()
        {
            try
            {
                var data = await _analyticsService.GetMonthlyRevenueAsync();
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar los ingresos mensuales.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("technical")]
        public async Task<IActionResult> GetTechnicalSummary([FromQuery] int? dateKey = null)
        {
            try
            {
                var data = await _analyticsService.GetTechnicalSummaryAsync(dateKey);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar el resumen analitico tecnico.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("technical/performance")]
        public async Task<IActionResult> GetTechnicalPerformance([FromQuery] int? dateKey = null)
        {
            try
            {
                var data = await _analyticsService.GetTechnicalPerformanceAsync(dateKey);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar el rendimiento tecnico.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("technical/heatmap")]
        public async Task<IActionResult> GetTechnicalHeatmap([FromQuery] int? dateKey = null)
        {
            try
            {
                var data = await _analyticsService.GetTechnicalHeatmapAsync(dateKey);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar el mapa de calor.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("technical/endpoints")]
        public async Task<IActionResult> GetTechnicalEndpoints([FromQuery] int? dateKey = null)
        {
            try
            {
                var data = await _analyticsService.GetTechnicalEndpointsAsync(dateKey);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar los endpoints tecnicos.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("appointments")]
        [HttpGet("citas")]
        public async Task<IActionResult> GetAppointmentsKpis([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetAppointmentsKpisAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar los KPIs de citas.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("appointments/summary")]
        [HttpGet("citas/summary")]
        public async Task<IActionResult> GetAppointmentsSummary([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetAppointmentsSummaryAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar el resumen de citas.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("appointments/doctors")]
        [HttpGet("citas/doctors")]
        public async Task<IActionResult> GetTopAppointmentDoctors([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetTopAppointmentDoctorsAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar el top de doctores en citas.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("appointments/status")]
        [HttpGet("citas/status")]
        public async Task<IActionResult> GetAppointmentStatuses([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetAppointmentStatusesAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar los estados de citas.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("appointments/specialties")]
        [HttpGet("citas/specialties")]
        public async Task<IActionResult> GetAppointmentSpecialties([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetAppointmentSpecialtiesAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar las especialidades en citas.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("appointments/recurrence")]
        [HttpGet("citas/recurrence")]
        public async Task<IActionResult> GetAppointmentPatientRecurrence([FromQuery] int? year = null, [FromQuery] int? month = null)
        {
            try
            {
                var data = await _analyticsService.GetAppointmentPatientRecurrenceAsync(year, month);
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar la recurrencia de pacientes en citas.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("appointments/monthly")]
        [HttpGet("citas/monthly")]
        public async Task<IActionResult> GetMonthlyAppointments()
        {
            try
            {
                var data = await _analyticsService.GetMonthlyAppointmentsAsync();
                return Ok(new
                {
                    status = "OK",
                    data
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    code = "500",
                    message = "Error al consultar las citas mensuales.",
                    details = ex.Message
                });
            }
        }
    }
}
