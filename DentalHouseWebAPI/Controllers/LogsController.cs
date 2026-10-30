using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DentalHouseWebAPI.Models;
using DentalHouseWebAPI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Administrador")]
    public class LogsController : ControllerBase
    {
        private readonly IMongoLogService _mongoLogService;
        private readonly ISystemLogService _sqlLogService;

        public LogsController(IMongoLogService mongoLogService, ISystemLogService sqlLogService)
        {
            _mongoLogService = mongoLogService;
            _sqlLogService = sqlLogService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? module = null,
            [FromQuery] string? action = null,
            [FromQuery] string? userName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null,
            [FromQuery] bool? isSuccess = null)
        {
            if (endDate.HasValue && endDate.Value.TimeOfDay == TimeSpan.Zero)
            {
                endDate = endDate.Value.Date.AddDays(1).AddTicks(-1);
            }

            var mongoResult = await _mongoLogService.GetPagedLogsAsync(
                pageNumber,
                pageSize,
                module,
                action,
                userName,
                startDate,
                endDate,
                searchTerm,
                isSuccess);

            if (mongoResult != null && mongoResult.TotalRecords > 0)
            {
                return Ok(new
                {
                    pageNumber = mongoResult.PageNumber,
                    pageSize = mongoResult.PageSize,
                    totalRecords = mongoResult.TotalRecords,
                    totalPages = mongoResult.TotalPages,
                    data = mongoResult.Data.Select(MapMongoLog)
                });
            }

            var sqlResponse = await _sqlLogService.GetPagedAsync(
                pageNumber,
                pageSize,
                module,
                action,
                userName,
                startDate,
                endDate,
                searchTerm,
                isSuccess);

            if (sqlResponse.IsSuccess && sqlResponse.Data != null)
            {
                return Ok(new
                {
                    pageNumber = sqlResponse.Data.PageNumber,
                    pageSize = sqlResponse.Data.PageSize,
                    totalRecords = sqlResponse.Data.TotalRecords,
                    totalPages = sqlResponse.Data.TotalPages,
                    data = sqlResponse.Data.Data
                });
            }

            return Ok(new
            {
                pageNumber = pageNumber,
                pageSize = pageSize,
                totalRecords = 0,
                totalPages = 1,
                data = new List<object>()
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                return BadRequest(new UnsuccessfulResponseDTO
                {
                    Code = "400",
                    Message = "El Id debe ser especificado."
                });
            }

            var mongoLog = await _mongoLogService.GetLogByIdAsync(id);
            if (mongoLog != null)
            {
                return Ok(MapMongoLog(mongoLog));
            }

            if (int.TryParse(id, out var intId) && intId > 0)
            {
                var sqlResponse = await _sqlLogService.GetByIdAsync(intId);
                if (sqlResponse.IsSuccess && sqlResponse.Data != null)
                {
                    return Ok(sqlResponse.Data);
                }
            }

            return NotFound(new UnsuccessfulResponseDTO
            {
                Code = "404",
                Message = "No se encontró el registro de log con el Id especificado."
            });
        }

        [HttpGet("today")]
        public async Task<IActionResult> GetToday(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 50)
        {
            var today = DateTime.Today;
            var endOfDay = today.AddDays(1).AddTicks(-1);
            return await GetAll(pageNumber, pageSize, startDate: today, endDate: endOfDay);
        }

        [HttpGet("errors")]
        public async Task<IActionResult> GetErrors(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? module = null,
            [FromQuery] string? action = null,
            [FromQuery] string? userName = null,
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? searchTerm = null)
        {
            return await GetAll(pageNumber, pageSize, module, action, userName, startDate, endDate, searchTerm, isSuccess: false);
        }

        [HttpGet("modules")]
        public async Task<IActionResult> GetModules()
        {
            var modules = await _mongoLogService.GetModulesAsync();
            return Ok(modules);
        }

        [HttpGet("actions")]
        public async Task<IActionResult> GetActions()
        {
            var actions = await _mongoLogService.GetActionsAsync();
            return Ok(actions);
        }

        private static object MapMongoLog(MongoSystemLog x)
        {
            var statusCode = x.Response?.StatusCode ?? 200;
            var isSuccessResult = statusCode > 0 ? (statusCode < 400) : (x.Level != "Error");
            var moduleName = ResolveModule(x.Request?.Path);
            var actionName = string.IsNullOrWhiteSpace(x.Request?.Method) ? "Auditoría" : x.Request.Method.ToUpperInvariant();
            var desc = $"[{actionName}] {x.Request?.Path} - {statusCode} ({x.Response?.ElapsedMilliseconds ?? 0} ms)";

            return new
            {
                id = x.Id ?? x.TraceId,
                logId = x.Id ?? x.TraceId,
                traceId = x.TraceId,
                timestamp = x.Timestamp,
                level = x.Level,
                environment = x.Environment,
                application = x.Application,
                clientIp = x.ClientIp ?? "127.0.0.1",
                ipAddress = x.ClientIp ?? "127.0.0.1",
                userId = string.IsNullOrWhiteSpace(x.UserId) ? "Sistema" : x.UserId,
                userName = string.IsNullOrWhiteSpace(x.UserId) ? "Sistema" : x.UserId,
                userRole = x.UserRole,
                clientPlatform = x.ClientPlatform,
                deviceId = x.DeviceId,
                method = actionName,
                path = x.Request?.Path ?? string.Empty,
                queryString = x.Request?.QueryString,
                payload = x.Request?.Payload,
                statusCode = statusCode,
                elapsedMilliseconds = x.Response?.ElapsedMilliseconds ?? 0,
                exceptionMessage = x.Exception?.Message,
                exceptionStackTrace = x.Exception?.StackTrace,
                exceptionSource = x.Exception?.Source,
                action = actionName,
                module = moduleName,
                description = desc,
                details = x.Exception?.Message ?? x.Request?.Payload,
                isSuccess = isSuccessResult,
                request = x.Request,
                response = x.Response,
                exception = x.Exception
            };
        }

        private static string ResolveModule(string? path)
        {
            if (string.IsNullOrWhiteSpace(path)) return "General";
            var p = path.ToLowerInvariant();
            if (p.Contains("/auth")) return "Autenticación";
            if (p.Contains("/patient")) return "Pacientes";
            if (p.Contains("/appointment") || p.Contains("/cita")) return "Citas";
            if (p.Contains("/doctor")) return "Doctores";
            if (p.Contains("/invoice")) return "Facturación";
            if (p.Contains("/sales") || p.Contains("/venta")) return "Ventas";
            if (p.Contains("/user") || p.Contains("/role")) return "Usuarios";
            if (p.Contains("/specialt")) return "Especialidades";
            if (p.Contains("/service")) return "Servicios";
            if (p.Contains("/analytics")) return "Analíticas";
            if (p.Contains("/log")) return "Logs";
            return "General";
        }
    }
}
