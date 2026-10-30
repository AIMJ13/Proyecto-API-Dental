using System;

namespace WebAPI.Business.DTOs
{
    public class SystemLogDto
    {
        public int LogId { get; set; }
        public DateTime Timestamp { get; set; }
        public int? UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? UserRole { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Module { get; set; } = string.Empty;
        public string? EntityId { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; }
        public string? IpAddress { get; set; }
        public bool IsSuccess { get; set; }
    }
}
