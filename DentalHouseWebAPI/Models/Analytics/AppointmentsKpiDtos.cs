using System.Collections.Generic;

namespace DentalHouseWebAPI.Models.Analytics
{
    public class AppointmentsKpisDto
    {
        public string Periodo { get; set; } = "Historico Global";
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int NoShowAppointments { get; set; }
        public decimal AttendanceRatePct { get; set; }
        public decimal CancellationRatePct { get; set; }
        public decimal? VolumeGrowthPct { get; set; }
        public PatientRecurrenceKpiDto PatientRecurrence { get; set; } = new();
        public List<DoctorAppointmentsKpiDto> TopDoctors { get; set; } = new();
        public List<AppointmentStatusKpiDto> AppointmentsByStatus { get; set; } = new();
        public List<SpecialtyAppointmentsKpiDto> AppointmentsBySpecialty { get; set; } = new();
        public List<MonthlyAppointmentsKpiDto> MonthlyAppointments { get; set; } = new();
    }

    public class AppointmentsSummaryKpiDto
    {
        public string Periodo { get; set; } = "Historico Global";
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public int PendingAppointments { get; set; }
        public int NoShowAppointments { get; set; }
        public decimal AttendanceRatePct { get; set; }
        public decimal CancellationRatePct { get; set; }
        public decimal? VolumeGrowthPct { get; set; }
    }

    public class DoctorAppointmentsKpiDto
    {
        public string DoctorName { get; set; } = string.Empty;
        public string SpecialtyName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public decimal AppointmentSharePct { get; set; }
    }

    public class AppointmentStatusKpiDto
    {
        public string StatusName { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal SharePct { get; set; }
    }

    public class SpecialtyAppointmentsKpiDto
    {
        public string SpecialtyName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public decimal SharePct { get; set; }
    }

    public class MonthlyAppointmentsKpiDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public int TotalAppointments { get; set; }
        public int CompletedAppointments { get; set; }
        public int CancelledAppointments { get; set; }
        public decimal? VolumeGrowthPct { get; set; }
    }
}
