using System.Collections.Generic;

namespace DentalHouseWebAPI.Models.Analytics
{
    public class BusinessKpisDto
    {
        public string Periodo { get; set; } = "Historico Global";
        public decimal TotalRevenue { get; set; }
        public int TotalSalesCount { get; set; }
        public decimal AverageTicket { get; set; }
        public int TotalServicesRendered { get; set; }
        public decimal? RevenueGrowthPct { get; set; }
        public decimal? VolumeVariationPct { get; set; }
        public PatientRecurrenceKpiDto PatientRecurrence { get; set; } = new();
        public List<DoctorRevenueKpiDto> TopDoctors { get; set; } = new();
        public List<ServiceRevenueKpiDto> TopServices { get; set; } = new();
        public List<SpecialtyRevenueKpiDto> RevenueBySpecialty { get; set; } = new();
        public List<MonthlyRevenueKpiDto> MonthlyRevenue { get; set; } = new();
    }

    public class BusinessSummaryKpiDto
    {
        public string Periodo { get; set; } = "Historico Global";
        public decimal TotalRevenue { get; set; }
        public int TotalSalesCount { get; set; }
        public decimal AverageTicket { get; set; }
        public int TotalServicesRendered { get; set; }
        public decimal? RevenueGrowthPct { get; set; }
        public decimal? VolumeVariationPct { get; set; }
    }

    public class DoctorRevenueKpiDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int ServicesCount { get; set; }
        public decimal RevenueSharePct { get; set; }
    }

    public class ServiceRevenueKpiDto
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal RevenueSharePct { get; set; }
        public decimal AverageRevenuePerService { get; set; }
    }

    public class SpecialtyRevenueKpiDto
    {
        public string SpecialtyName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public decimal RevenueSharePct { get; set; }
    }

    public class PatientRecurrenceKpiDto
    {
        public int TotalPatients { get; set; }
        public int RepeatPatients { get; set; }
        public decimal RecurrenceRatePct { get; set; }
    }

    public class MonthlyRevenueKpiDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal TotalRevenue { get; set; }
        public int TotalSales { get; set; }
        public int TotalServices { get; set; }
        public decimal? RevenueGrowthPct { get; set; }
        public decimal? VolumeVariationPct { get; set; }
    }
}
