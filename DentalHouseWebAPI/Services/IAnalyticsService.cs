using System.Collections.Generic;
using System.Threading.Tasks;
using DentalHouseWebAPI.Models.Analytics;

namespace DentalHouseWebAPI.Services
{
    public interface IAnalyticsService
    {
        Task<BusinessKpisDto> GetBusinessKpisAsync(int? year = null, int? month = null);
        Task<BusinessSummaryKpiDto> GetSummaryAsync(int? year = null, int? month = null);
        Task<List<DoctorRevenueKpiDto>> GetTopDoctorsAsync(int? year = null, int? month = null);
        Task<List<ServiceRevenueKpiDto>> GetTopServicesAsync(int? year = null, int? month = null);
        Task<List<SpecialtyRevenueKpiDto>> GetSpecialtiesAsync(int? year = null, int? month = null);
        Task<PatientRecurrenceKpiDto> GetPatientRecurrenceAsync(int? year = null, int? month = null);
        Task<List<MonthlyRevenueKpiDto>> GetMonthlyRevenueAsync();
        Task<TechnicalPerformanceKpiDto> GetTechnicalPerformanceAsync(int? dateKey = null);
        Task<List<TechnicalHeatmapKpiDto>> GetTechnicalHeatmapAsync(int? dateKey = null);
        Task<List<TechnicalEndpointKpiDto>> GetTechnicalEndpointsAsync(int? dateKey = null);
        Task<TechnicalSummaryKpiDto> GetTechnicalSummaryAsync(int? dateKey = null);
        Task<AppointmentsKpisDto> GetAppointmentsKpisAsync(int? year = null, int? month = null);
        Task<AppointmentsSummaryKpiDto> GetAppointmentsSummaryAsync(int? year = null, int? month = null);
        Task<List<DoctorAppointmentsKpiDto>> GetTopAppointmentDoctorsAsync(int? year = null, int? month = null);
        Task<List<AppointmentStatusKpiDto>> GetAppointmentStatusesAsync(int? year = null, int? month = null);
        Task<List<SpecialtyAppointmentsKpiDto>> GetAppointmentSpecialtiesAsync(int? year = null, int? month = null);
        Task<PatientRecurrenceKpiDto> GetAppointmentPatientRecurrenceAsync(int? year = null, int? month = null);
        Task<List<MonthlyAppointmentsKpiDto>> GetMonthlyAppointmentsAsync();
    }
}
