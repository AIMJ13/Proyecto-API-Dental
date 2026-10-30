using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace DentalHouseWebAPI.Helpers
{
    public static class HttpContextExtensions
    {
        public static (int? userId, string? userName, string? userRole, string? ipAddress) GetAuditUserInfo(this HttpContext context)
        {
            int? userId = null;
            var subClaim = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? context.User?.FindFirst("sub")?.Value;
            if (int.TryParse(subClaim, out var parsedId))
                userId = parsedId;

            var userName = context.User?.Identity?.Name
                           ?? context.User?.FindFirst("name")?.Value
                           ?? context.User?.FindFirst(ClaimTypes.Name)?.Value;

            var userRole = context.User?.FindFirst(ClaimTypes.Role)?.Value
                           ?? context.User?.FindFirst("role")?.Value;

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();

            return (userId, userName, userRole, ipAddress);
        }
    }
}
