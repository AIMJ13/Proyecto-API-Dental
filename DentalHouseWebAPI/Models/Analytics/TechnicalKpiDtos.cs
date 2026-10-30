using System.Collections.Generic;

namespace DentalHouseWebAPI.Models.Analytics
{
    public class TechnicalPerformanceKpiDto
    {
        public string Periodo { get; set; } = "Global";
        public int TotalRequests { get; set; }
        public decimal AvgResponseTimeMs { get; set; }
        public decimal ErrorRatePct { get; set; }
        public int TotalErrors500 { get; set; }
        public int TotalClientErrors4xx { get; set; }
        public string SystemStatus { get; set; } = "Healthy";
    }

    public class TechnicalHeatmapKpiDto
    {
        public int Hour { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public string SystemModule { get; set; } = string.Empty;
        public int TotalIncidents { get; set; }
        public decimal AvgResponseTimeMs { get; set; }
    }

    public class TechnicalEndpointKpiDto
    {
        public string EndpointRoute { get; set; } = string.Empty;
        public string HttpMethod { get; set; } = string.Empty;
        public string SystemModule { get; set; } = string.Empty;
        public int TotalRequests { get; set; }
        public decimal AvgResponseTimeMs { get; set; }
        public int ErrorCount { get; set; }
        public decimal ErrorRatePct { get; set; }
    }

    public class TechnicalSummaryKpiDto
    {
        public TechnicalPerformanceKpiDto Performance { get; set; } = new();
        public List<TechnicalEndpointKpiDto> TopEndpoints { get; set; } = new();
        public List<TechnicalHeatmapKpiDto> Heatmap { get; set; } = new();
    }
}
