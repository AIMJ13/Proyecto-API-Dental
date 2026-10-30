using System;
using System.Diagnostics;
using System.IO;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DentalHouseWebAPI.Models;
using DentalHouseWebAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

namespace DentalHouseWebAPI.Middlewares
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IHostEnvironment _env;

        public AuditLoggingMiddleware(RequestDelegate next, IHostEnvironment env)
        {
            _next = next;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context, IMongoLogService logService)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }

            var timer = Stopwatch.StartNew();
            var traceId = context.TraceIdentifier;

            context.Request.EnableBuffering();
            string requestBody = string.Empty;

            if (context.Request.ContentLength > 0 && context.Request.Body.CanRead)
            {
                using var reader = new StreamReader(
                    context.Request.Body,
                    Encoding.UTF8,
                    detectEncodingFromByteOrderMarks: false,
                    leaveOpen: true);

                requestBody = await reader.ReadToEndAsync();
                context.Request.Body.Position = 0;
            }

            requestBody = SanitizarPayload(requestBody);

            Exception? caughtException = null;

            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                caughtException = ex;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync($"{{\"error\":\"Error interno en el servidor.\", \"traceId\":\"{traceId}\"}}");
            }
            finally
            {
                timer.Stop();

                var clientPlatform = context.Request.Headers["X-Client-Platform"].ToString();
                if (string.IsNullOrWhiteSpace(clientPlatform))
                {
                    var userAgent = context.Request.Headers["User-Agent"].ToString();
                    clientPlatform = userAgent.Contains("Dart", StringComparison.OrdinalIgnoreCase) ? "Flutter-Android" : "Web";
                }

                var deviceId = context.Request.Headers["X-Device-Id"].ToString();
                if (string.IsNullOrWhiteSpace(deviceId))
                {
                    deviceId = null;
                }

                var log = new MongoSystemLog
                {
                    TraceId = traceId,
                    Timestamp = DateTime.UtcNow,
                    Environment = _env.EnvironmentName,
                    Level = caughtException != null ? "Error" : (context.Response.StatusCode >= 400 ? "Warning" : "Information"),
                    ClientIp = context.Connection.RemoteIpAddress?.ToString(),
                    UserId = context.User?.Identity?.IsAuthenticated == true ? context.User.Identity.Name : "Anonimo",
                    Application = "DentalHouseBackend",
                    ClientPlatform = clientPlatform,
                    DeviceId = deviceId,
                    UserRole = context.User?.FindFirst(ClaimTypes.Role)?.Value,
                    Request = new MongoRequestDetail
                    {
                        Method = context.Request.Method,
                        Path = context.Request.Path,
                        QueryString = context.Request.QueryString.HasValue ? context.Request.QueryString.Value : null,
                        Payload = string.IsNullOrWhiteSpace(requestBody) ? null : requestBody
                    },
                    Response = new MongoResponseDetail
                    {
                        StatusCode = context.Response.StatusCode,
                        ElapsedMilliseconds = timer.ElapsedMilliseconds
                    }
                };

                if (caughtException != null)
                {
                    log.Exception = new MongoExceptionDetail
                    {
                        Message = caughtException.Message,
                        StackTrace = caughtException.StackTrace,
                        Source = caughtException.Source ?? string.Empty
                    };
                }

                _ = logService.InsertLogAsync(log);
            }
        }

        private static string SanitizarPayload(string payload)
        {
            if (string.IsNullOrWhiteSpace(payload)) return payload;

            var pattern = @"(?i)""(password|contraseña|token|secret|clave)""\s*:\s*""([^""]+)""";
            return Regex.Replace(payload, pattern, "\"$1\": \"******\"");
        }
    }
}
