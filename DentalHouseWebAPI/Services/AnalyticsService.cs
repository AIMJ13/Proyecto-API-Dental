using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DentalHouseWebAPI.Models.Analytics;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DentalHouseWebAPI.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly string _connectionString;
        private readonly string _technicalConnectionString;
        private readonly string _citasConnectionString;

        public AnalyticsService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("AnalyticsDWConnection")
                ?? "Server=localhost;Database=DentalHouseDW;Integrated Security=True;TrustServerCertificate=True;";
            _technicalConnectionString = configuration.GetConnectionString("TechnicalDWConnection")
                ?? "Server=localhost;Database=DentalHouseDW_Tecnico;Integrated Security=True;TrustServerCertificate=True;";
            _citasConnectionString = configuration.GetConnectionString("CitasDWConnection")
                ?? "Server=localhost;Database=DentalHouseDW_Citas;Integrated Security=True;TrustServerCertificate=True;";
        }

        public async Task<BusinessKpisDto> GetBusinessKpisAsync(int? year = null, int? month = null)
        {
            var summary = await GetSummaryAsync(year, month);
            var doctors = await GetTopDoctorsAsync(year, month);
            var services = await GetTopServicesAsync(year, month);
            var specialties = await GetSpecialtiesAsync(year, month);
            var recurrence = await GetPatientRecurrenceAsync(year, month);
            var monthly = await GetMonthlyRevenueAsync();

            return new BusinessKpisDto
            {
                Periodo = summary.Periodo,
                TotalRevenue = summary.TotalRevenue,
                TotalSalesCount = summary.TotalSalesCount,
                AverageTicket = summary.AverageTicket,
                TotalServicesRendered = summary.TotalServicesRendered,
                RevenueGrowthPct = summary.RevenueGrowthPct,
                VolumeVariationPct = summary.VolumeVariationPct,
                PatientRecurrence = recurrence,
                TopDoctors = doctors,
                TopServices = services,
                RevenueBySpecialty = specialties,
                MonthlyRevenue = monthly
            };
        }

        public async Task<BusinessSummaryKpiDto> GetSummaryAsync(int? year = null, int? month = null)
        {
            var result = new BusinessSummaryKpiDto();
            result.Periodo = (year.HasValue && month.HasValue) 
                ? $"Mes {month.Value:D2}/{year.Value}" 
                : (year.HasValue ? $"Año {year.Value}" : "Historico Global");

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        ISNULL(SUM(f.TotalRevenue), 0) AS TotalRevenue,
                        COUNT(DISTINCT f.SaleId) AS TotalSalesCount,
                        ISNULL(AVG(f.TotalRevenue), 0) AS AverageTicket,
                        ISNULL(SUM(f.Quantity), 0) AS TotalServices
                    FROM Fact_Sales f
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result.TotalRevenue = reader["TotalRevenue"] != DBNull.Value ? Convert.ToDecimal(reader["TotalRevenue"]) : 0;
                    result.TotalSalesCount = reader["TotalSalesCount"] != DBNull.Value ? Convert.ToInt32(reader["TotalSalesCount"]) : 0;
                    result.AverageTicket = reader["AverageTicket"] != DBNull.Value ? Math.Round(Convert.ToDecimal(reader["AverageTicket"]), 2) : 0;
                    result.TotalServicesRendered = reader["TotalServices"] != DBNull.Value ? Convert.ToInt32(reader["TotalServices"]) : 0;
                }
            }

            if (year.HasValue && month.HasValue)
            {
                var monthlyList = await GetMonthlyRevenueAsync();
                var currentIndex = monthlyList.FindIndex(m => m.Year == year.Value && m.Month == month.Value);
                if (currentIndex >= 0)
                {
                    result.RevenueGrowthPct = monthlyList[currentIndex].RevenueGrowthPct;
                    result.VolumeVariationPct = monthlyList[currentIndex].VolumeVariationPct;
                }
            }

            return result;
        }

        public async Task<List<DoctorRevenueKpiDto>> GetTopDoctorsAsync(int? year = null, int? month = null)
        {
            var list = new List<DoctorRevenueKpiDto>();
            decimal totalRevenue = 0;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT TOP 5 
                        d.FullName AS DoctorName,
                        d.SpecialtyName,
                        ISNULL(SUM(f.TotalRevenue), 0) AS TotalRevenue,
                        ISNULL(SUM(f.Quantity), 0) AS ServicesCount
                    FROM Fact_Sales f
                    JOIN Dim_Doctor d ON f.DoctorKey = d.DoctorKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY d.FullName, d.SpecialtyName
                    ORDER BY TotalRevenue DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var rev = Convert.ToDecimal(reader["TotalRevenue"]);
                    totalRevenue += rev;
                    list.Add(new DoctorRevenueKpiDto
                    {
                        DoctorName = reader["DoctorName"].ToString() ?? string.Empty,
                        SpecialtyName = reader["SpecialtyName"].ToString() ?? string.Empty,
                        TotalRevenue = rev,
                        ServicesCount = Convert.ToInt32(reader["ServicesCount"])
                    });
                }
            }

            var globalSummary = await GetSummaryAsync(year, month);
            var baseRevenue = globalSummary.TotalRevenue > 0 ? globalSummary.TotalRevenue : totalRevenue;

            if (baseRevenue > 0)
            {
                foreach (var doc in list)
                {
                    doc.RevenueSharePct = Math.Round((doc.TotalRevenue / baseRevenue) * 100, 2);
                }
            }

            return list;
        }

        public async Task<List<ServiceRevenueKpiDto>> GetTopServicesAsync(int? year = null, int? month = null)
        {
            var list = new List<ServiceRevenueKpiDto>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT TOP 5 
                        s.ServiceName,
                        ISNULL(SUM(f.Quantity), 0) AS Quantity,
                        ISNULL(SUM(f.TotalRevenue), 0) AS TotalRevenue
                    FROM Fact_Sales f
                    JOIN Dim_Service s ON f.ServiceKey = s.ServiceKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY s.ServiceName
                    ORDER BY TotalRevenue DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var qty = Convert.ToInt32(reader["Quantity"]);
                    var rev = Convert.ToDecimal(reader["TotalRevenue"]);
                    var avg = qty > 0 ? Math.Round(rev / qty, 2) : 0;

                    list.Add(new ServiceRevenueKpiDto
                    {
                        ServiceName = reader["ServiceName"].ToString() ?? string.Empty,
                        Quantity = qty,
                        TotalRevenue = rev,
                        AverageRevenuePerService = avg
                    });
                }
            }

            var globalSummary = await GetSummaryAsync(year, month);
            if (globalSummary.TotalRevenue > 0)
            {
                foreach (var srv in list)
                {
                    srv.RevenueSharePct = Math.Round((srv.TotalRevenue / globalSummary.TotalRevenue) * 100, 2);
                }
            }

            return list;
        }

        public async Task<List<SpecialtyRevenueKpiDto>> GetSpecialtiesAsync(int? year = null, int? month = null)
        {
            var list = new List<SpecialtyRevenueKpiDto>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        ISNULL(d.SpecialtyName, 'General') AS SpecialtyName,
                        ISNULL(SUM(f.TotalRevenue), 0) AS TotalRevenue
                    FROM Fact_Sales f
                    JOIN Dim_Doctor d ON f.DoctorKey = d.DoctorKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY d.SpecialtyName
                    ORDER BY TotalRevenue DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    list.Add(new SpecialtyRevenueKpiDto
                    {
                        SpecialtyName = reader["SpecialtyName"].ToString() ?? string.Empty,
                        TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"])
                    });
                }
            }

            var globalSummary = await GetSummaryAsync(year, month);
            if (globalSummary.TotalRevenue > 0)
            {
                foreach (var esp in list)
                {
                    esp.RevenueSharePct = Math.Round((esp.TotalRevenue / globalSummary.TotalRevenue) * 100, 2);
                }
            }

            return list;
        }

        public async Task<PatientRecurrenceKpiDto> GetPatientRecurrenceAsync(int? year = null, int? month = null)
        {
            var result = new PatientRecurrenceKpiDto();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    COUNT(DISTINCT PatientKey) AS TotalPatients,
                    COUNT(DISTINCT CASE WHEN VisitCount > 1 THEN PatientKey END) AS RepeatPatients
                FROM (
                    SELECT f.PatientKey, COUNT(DISTINCT f.SaleId) AS VisitCount
                    FROM Fact_Sales f
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY f.PatientKey
                ) sub;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var total = reader["TotalPatients"] != DBNull.Value ? Convert.ToInt32(reader["TotalPatients"]) : 0;
                var repeat = reader["RepeatPatients"] != DBNull.Value ? Convert.ToInt32(reader["RepeatPatients"]) : 0;
                var rate = total > 0 ? Math.Round(((decimal)repeat / total) * 100, 2) : 0;

                result.TotalPatients = total;
                result.RepeatPatients = repeat;
                result.RecurrenceRatePct = rate;
            }

            return result;
        }

        public async Task<List<MonthlyRevenueKpiDto>> GetMonthlyRevenueAsync()
        {
            var list = new List<MonthlyRevenueKpiDto>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        t.Year,
                        t.Month,
                        t.MonthName,
                        ISNULL(SUM(f.TotalRevenue), 0) AS TotalRevenue,
                        COUNT(DISTINCT f.SaleId) AS TotalSales,
                        ISNULL(SUM(f.Quantity), 0) AS TotalServices
                    FROM Fact_Sales f
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    GROUP BY t.Year, t.Month, t.MonthName
                    ORDER BY t.Year, t.Month;";

                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new MonthlyRevenueKpiDto
                    {
                        Year = Convert.ToInt32(reader["Year"]),
                        Month = Convert.ToInt32(reader["Month"]),
                        MonthName = reader["MonthName"].ToString() ?? string.Empty,
                        TotalRevenue = Convert.ToDecimal(reader["TotalRevenue"]),
                        TotalSales = Convert.ToInt32(reader["TotalSales"]),
                        TotalServices = Convert.ToInt32(reader["TotalServices"])
                    });
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    var prevRevenue = list[i - 1].TotalRevenue;
                    if (prevRevenue > 0)
                    {
                        list[i].RevenueGrowthPct = Math.Round(((list[i].TotalRevenue - prevRevenue) / prevRevenue) * 100, 2);
                    }

                    var prevServices = list[i - 1].TotalServices;
                    if (prevServices > 0)
                    {
                        list[i].VolumeVariationPct = Math.Round(((decimal)(list[i].TotalServices - prevServices) / prevServices) * 100, 2);
                    }
                }
            }

            return list;
        }

        public async Task<TechnicalPerformanceKpiDto> GetTechnicalPerformanceAsync(int? dateKey = null)
        {
            var result = new TechnicalPerformanceKpiDto();
            if (dateKey.HasValue)
            {
                result.Periodo = $"Date {dateKey.Value}";
            }

            using var conn = new SqlConnection(_technicalConnectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    COUNT(*) AS TotalRequests,
                    ISNULL(AVG(f.ResponseTimeMs), 0) AS AvgResponseTimeMs,
                    CASE WHEN COUNT(*) > 0 THEN (CAST(SUM(CASE WHEN f.IsError = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / COUNT(*)) * 100.0 ELSE 0 END AS ErrorRatePct,
                    ISNULL(SUM(CASE WHEN el.StatusCode >= 500 THEN 1 ELSE 0 END), 0) AS TotalErrors500,
                    ISNULL(SUM(CASE WHEN el.StatusCode >= 400 AND el.StatusCode < 500 THEN 1 ELSE 0 END), 0) AS TotalClientErrors4xx
                FROM Fact_Operational_Events f
                LEFT JOIN Dim_Log_Event_Type el ON f.EventKey = el.EventKey
                WHERE (@dateKey IS NULL OR f.DateKey = @dateKey);";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dateKey", (object?)dateKey ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                result.TotalRequests = reader.GetInt32(0);
                result.AvgResponseTimeMs = Math.Round(reader.GetDecimal(1), 2);
                result.ErrorRatePct = Math.Round(reader.GetDecimal(2), 2);
                result.TotalErrors500 = reader.GetInt32(3);
                result.TotalClientErrors4xx = reader.GetInt32(4);

                if (result.ErrorRatePct < 2.5m)
                {
                    result.SystemStatus = "Healthy";
                }
                else if (result.ErrorRatePct <= 5.0m)
                {
                    result.SystemStatus = "Degraded";
                }
                else
                {
                    result.SystemStatus = "Critical";
                }
            }

            return result;
        }

        public async Task<List<TechnicalHeatmapKpiDto>> GetTechnicalHeatmapAsync(int? dateKey = null)
        {
            var list = new List<TechnicalHeatmapKpiDto>();

            using var conn = new SqlConnection(_technicalConnectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    fh.Hour,
                    fh.TimeSlot,
                    ISNULL(ep.SystemModule, 'General') AS SystemModule,
                    COUNT(*) AS TotalIncidents,
                    ISNULL(AVG(f.ResponseTimeMs), 0) AS AvgResponseTimeMs
                FROM Fact_Operational_Events f
                INNER JOIN Dim_Date_Time fh ON f.DateKey = fh.DateKey
                LEFT JOIN Dim_Endpoint ep ON f.EndpointKey = ep.EndpointKey
                WHERE (@dateKey IS NULL OR f.DateKey = @dateKey)
                GROUP BY fh.Hour, fh.TimeSlot, ep.SystemModule
                ORDER BY fh.Hour ASC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dateKey", (object?)dateKey ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TechnicalHeatmapKpiDto
                {
                    Hour = reader.GetInt32(0),
                    TimeSlot = reader.GetString(1),
                    SystemModule = reader.GetString(2),
                    TotalIncidents = reader.GetInt32(3),
                    AvgResponseTimeMs = Math.Round(reader.GetDecimal(4), 2)
                });
            }

            return list;
        }

        public async Task<List<TechnicalEndpointKpiDto>> GetTechnicalEndpointsAsync(int? dateKey = null)
        {
            var list = new List<TechnicalEndpointKpiDto>();

            using var conn = new SqlConnection(_technicalConnectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    ISNULL(ep.EndpointRoute, '/unknown') AS EndpointRoute,
                    ISNULL(ep.HttpMethod, 'GET') AS HttpMethod,
                    ISNULL(ep.SystemModule, 'General') AS SystemModule,
                    COUNT(*) AS TotalRequests,
                    ISNULL(AVG(f.ResponseTimeMs), 0) AS AvgResponseTimeMs,
                    SUM(CASE WHEN f.IsError = 1 THEN 1 ELSE 0 END) AS ErrorCount,
                    CASE WHEN COUNT(*) > 0 THEN (CAST(SUM(CASE WHEN f.IsError = 1 THEN 1 ELSE 0 END) AS DECIMAL(10,2)) / COUNT(*)) * 100.0 ELSE 0 END AS ErrorRatePct
                FROM Fact_Operational_Events f
                LEFT JOIN Dim_Endpoint ep ON f.EndpointKey = ep.EndpointKey
                WHERE (@dateKey IS NULL OR f.DateKey = @dateKey)
                GROUP BY ep.EndpointRoute, ep.HttpMethod, ep.SystemModule
                ORDER BY TotalRequests DESC;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@dateKey", (object?)dateKey ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new TechnicalEndpointKpiDto
                {
                    EndpointRoute = reader.GetString(0),
                    HttpMethod = reader.GetString(1),
                    SystemModule = reader.GetString(2),
                    TotalRequests = reader.GetInt32(3),
                    AvgResponseTimeMs = Math.Round(reader.GetDecimal(4), 2),
                    ErrorCount = reader.GetInt32(5),
                    ErrorRatePct = Math.Round(reader.GetDecimal(6), 2)
                });
            }

            return list;
        }

        public async Task<TechnicalSummaryKpiDto> GetTechnicalSummaryAsync(int? dateKey = null)
        {
            var performance = await GetTechnicalPerformanceAsync(dateKey);
            var endpoints = await GetTechnicalEndpointsAsync(dateKey);
            var heatmap = await GetTechnicalHeatmapAsync(dateKey);

            return new TechnicalSummaryKpiDto
            {
                Performance = performance,
                TopEndpoints = endpoints,
                Heatmap = heatmap
            };
        }

        public async Task<AppointmentsKpisDto> GetAppointmentsKpisAsync(int? year = null, int? month = null)
        {
            var summary = await GetAppointmentsSummaryAsync(year, month);
            var doctors = await GetTopAppointmentDoctorsAsync(year, month);
            var statuses = await GetAppointmentStatusesAsync(year, month);
            var specialties = await GetAppointmentSpecialtiesAsync(year, month);
            var recurrence = await GetAppointmentPatientRecurrenceAsync(year, month);
            var monthly = await GetMonthlyAppointmentsAsync();

            return new AppointmentsKpisDto
            {
                Periodo = summary.Periodo,
                TotalAppointments = summary.TotalAppointments,
                CompletedAppointments = summary.CompletedAppointments,
                CancelledAppointments = summary.CancelledAppointments,
                PendingAppointments = summary.PendingAppointments,
                NoShowAppointments = summary.NoShowAppointments,
                AttendanceRatePct = summary.AttendanceRatePct,
                CancellationRatePct = summary.CancellationRatePct,
                VolumeGrowthPct = summary.VolumeGrowthPct,
                PatientRecurrence = recurrence,
                TopDoctors = doctors,
                AppointmentsByStatus = statuses,
                AppointmentsBySpecialty = specialties,
                MonthlyAppointments = monthly
            };
        }

        public async Task<AppointmentsSummaryKpiDto> GetAppointmentsSummaryAsync(int? year = null, int? month = null)
        {
            var result = new AppointmentsSummaryKpiDto();
            result.Periodo = (year.HasValue && month.HasValue)
                ? $"Mes {month.Value:D2}/{year.Value}"
                : (year.HasValue ? $"Año {year.Value}" : "Historico Global");

            using (var conn = new SqlConnection(_citasConnectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        COUNT(f.AppointmentKey) AS TotalAppointments,
                        ISNULL(SUM(CASE WHEN s.StatusName = 'Completada' THEN 1 ELSE 0 END), 0) AS CompletedAppointments,
                        ISNULL(SUM(CASE WHEN s.StatusName = 'Cancelada' THEN 1 ELSE 0 END), 0) AS CancelledAppointments,
                        ISNULL(SUM(CASE WHEN s.StatusName = 'Programada' THEN 1 ELSE 0 END), 0) AS PendingAppointments,
                        ISNULL(SUM(CASE WHEN s.StatusName = 'No asistió' THEN 1 ELSE 0 END), 0) AS NoShowAppointments
                    FROM Fact_Appointments f
                    JOIN Dim_Appointment_Status s ON f.StatusKey = s.StatusKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result.TotalAppointments = reader["TotalAppointments"] != DBNull.Value ? Convert.ToInt32(reader["TotalAppointments"]) : 0;
                    result.CompletedAppointments = reader["CompletedAppointments"] != DBNull.Value ? Convert.ToInt32(reader["CompletedAppointments"]) : 0;
                    result.CancelledAppointments = reader["CancelledAppointments"] != DBNull.Value ? Convert.ToInt32(reader["CancelledAppointments"]) : 0;
                    result.PendingAppointments = reader["PendingAppointments"] != DBNull.Value ? Convert.ToInt32(reader["PendingAppointments"]) : 0;
                    result.NoShowAppointments = reader["NoShowAppointments"] != DBNull.Value ? Convert.ToInt32(reader["NoShowAppointments"]) : 0;

                    if (result.TotalAppointments > 0)
                    {
                        result.AttendanceRatePct = Math.Round(((decimal)result.CompletedAppointments / result.TotalAppointments) * 100, 2);
                        result.CancellationRatePct = Math.Round(((decimal)result.CancelledAppointments / result.TotalAppointments) * 100, 2);
                    }
                }
            }

            if (year.HasValue && month.HasValue)
            {
                var monthlyList = await GetMonthlyAppointmentsAsync();
                var currentIndex = monthlyList.FindIndex(m => m.Year == year.Value && m.Month == month.Value);
                if (currentIndex >= 0)
                {
                    result.VolumeGrowthPct = monthlyList[currentIndex].VolumeGrowthPct;
                }
            }

            return result;
        }

        public async Task<List<DoctorAppointmentsKpiDto>> GetTopAppointmentDoctorsAsync(int? year = null, int? month = null)
        {
            var list = new List<DoctorAppointmentsKpiDto>();
            int grandTotal = 0;

            using (var conn = new SqlConnection(_citasConnectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT TOP 5 
                        d.FullName AS DoctorName,
                        ISNULL(d.SpecialtyName, 'General') AS SpecialtyName,
                        COUNT(f.AppointmentKey) AS TotalAppointments,
                        ISNULL(SUM(CASE WHEN s.StatusName = 'Completada' THEN 1 ELSE 0 END), 0) AS CompletedAppointments
                    FROM Fact_Appointments f
                    JOIN Dim_Doctor d ON f.DoctorKey = d.DoctorKey
                    JOIN Dim_Appointment_Status s ON f.StatusKey = s.StatusKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY d.FullName, d.SpecialtyName
                    ORDER BY TotalAppointments DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var total = Convert.ToInt32(reader["TotalAppointments"]);
                    grandTotal += total;
                    list.Add(new DoctorAppointmentsKpiDto
                    {
                        DoctorName = reader["DoctorName"].ToString() ?? string.Empty,
                        SpecialtyName = reader["SpecialtyName"].ToString() ?? string.Empty,
                        TotalAppointments = total,
                        CompletedAppointments = Convert.ToInt32(reader["CompletedAppointments"])
                    });
                }
            }

            var summary = await GetAppointmentsSummaryAsync(year, month);
            var baseTotal = summary.TotalAppointments > 0 ? summary.TotalAppointments : grandTotal;

            if (baseTotal > 0)
            {
                foreach (var doc in list)
                {
                    doc.AppointmentSharePct = Math.Round(((decimal)doc.TotalAppointments / baseTotal) * 100, 2);
                }
            }

            return list;
        }

        public async Task<List<AppointmentStatusKpiDto>> GetAppointmentStatusesAsync(int? year = null, int? month = null)
        {
            var list = new List<AppointmentStatusKpiDto>();
            int grandTotal = 0;

            using (var conn = new SqlConnection(_citasConnectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        s.StatusName,
                        COUNT(f.AppointmentKey) AS TotalAppointments
                    FROM Fact_Appointments f
                    JOIN Dim_Appointment_Status s ON f.StatusKey = s.StatusKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY s.StatusName
                    ORDER BY TotalAppointments DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var cnt = Convert.ToInt32(reader["TotalAppointments"]);
                    grandTotal += cnt;
                    list.Add(new AppointmentStatusKpiDto
                    {
                        StatusName = reader["StatusName"].ToString() ?? string.Empty,
                        Count = cnt
                    });
                }
            }

            var summary = await GetAppointmentsSummaryAsync(year, month);
            var baseTotal = summary.TotalAppointments > 0 ? summary.TotalAppointments : grandTotal;

            if (baseTotal > 0)
            {
                foreach (var st in list)
                {
                    st.SharePct = Math.Round(((decimal)st.Count / baseTotal) * 100, 2);
                }
            }

            return list;
        }

        public async Task<List<SpecialtyAppointmentsKpiDto>> GetAppointmentSpecialtiesAsync(int? year = null, int? month = null)
        {
            var list = new List<SpecialtyAppointmentsKpiDto>();
            int grandTotal = 0;

            using (var conn = new SqlConnection(_citasConnectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        ISNULL(d.SpecialtyName, 'General') AS SpecialtyName,
                        COUNT(f.AppointmentKey) AS TotalAppointments
                    FROM Fact_Appointments f
                    JOIN Dim_Doctor d ON f.DoctorKey = d.DoctorKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY d.SpecialtyName
                    ORDER BY TotalAppointments DESC;";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var cnt = Convert.ToInt32(reader["TotalAppointments"]);
                    grandTotal += cnt;
                    list.Add(new SpecialtyAppointmentsKpiDto
                    {
                        SpecialtyName = reader["SpecialtyName"].ToString() ?? string.Empty,
                        TotalAppointments = cnt
                    });
                }
            }

            var summary = await GetAppointmentsSummaryAsync(year, month);
            var baseTotal = summary.TotalAppointments > 0 ? summary.TotalAppointments : grandTotal;

            if (baseTotal > 0)
            {
                foreach (var esp in list)
                {
                    esp.SharePct = Math.Round(((decimal)esp.TotalAppointments / baseTotal) * 100, 2);
                }
            }

            return list;
        }

        public async Task<PatientRecurrenceKpiDto> GetAppointmentPatientRecurrenceAsync(int? year = null, int? month = null)
        {
            var result = new PatientRecurrenceKpiDto();

            using var conn = new SqlConnection(_citasConnectionString);
            await conn.OpenAsync();

            var sql = @"
                SELECT 
                    COUNT(DISTINCT PatientKey) AS TotalPatients,
                    COUNT(DISTINCT CASE WHEN AppointmentCount > 1 THEN PatientKey END) AS RepeatPatients
                FROM (
                    SELECT f.PatientKey, COUNT(f.AppointmentKey) AS AppointmentCount
                    FROM Fact_Appointments f
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    WHERE (@Year IS NULL OR t.Year = @Year)
                      AND (@Month IS NULL OR t.Month = @Month)
                    GROUP BY f.PatientKey
                ) sub;";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Year", (object?)year ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Month", (object?)month ?? DBNull.Value);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                var total = reader["TotalPatients"] != DBNull.Value ? Convert.ToInt32(reader["TotalPatients"]) : 0;
                var repeat = reader["RepeatPatients"] != DBNull.Value ? Convert.ToInt32(reader["RepeatPatients"]) : 0;
                var rate = total > 0 ? Math.Round(((decimal)repeat / total) * 100, 2) : 0;

                result.TotalPatients = total;
                result.RepeatPatients = repeat;
                result.RecurrenceRatePct = rate;
            }

            return result;
        }

        public async Task<List<MonthlyAppointmentsKpiDto>> GetMonthlyAppointmentsAsync()
        {
            var list = new List<MonthlyAppointmentsKpiDto>();

            using (var conn = new SqlConnection(_citasConnectionString))
            {
                await conn.OpenAsync();

                var sql = @"
                    SELECT 
                        t.Year,
                        t.Month,
                        t.MonthName,
                        COUNT(f.AppointmentKey) AS TotalAppointments,
                        ISNULL(SUM(CASE WHEN s.StatusName = 'Completada' THEN 1 ELSE 0 END), 0) AS CompletedAppointments,
                        ISNULL(SUM(CASE WHEN s.StatusName = 'Cancelada' THEN 1 ELSE 0 END), 0) AS CancelledAppointments
                    FROM Fact_Appointments f
                    JOIN Dim_Appointment_Status s ON f.StatusKey = s.StatusKey
                    JOIN Dim_Time t ON f.TimeKey = t.TimeKey
                    GROUP BY t.Year, t.Month, t.MonthName
                    ORDER BY t.Year, t.Month;";

                using var cmd = new SqlCommand(sql, conn);
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new MonthlyAppointmentsKpiDto
                    {
                        Year = Convert.ToInt32(reader["Year"]),
                        Month = Convert.ToInt32(reader["Month"]),
                        MonthName = reader["MonthName"].ToString() ?? string.Empty,
                        TotalAppointments = Convert.ToInt32(reader["TotalAppointments"]),
                        CompletedAppointments = Convert.ToInt32(reader["CompletedAppointments"]),
                        CancelledAppointments = Convert.ToInt32(reader["CancelledAppointments"])
                    });
                }
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                {
                    var prevAppointments = list[i - 1].TotalAppointments;
                    if (prevAppointments > 0)
                    {
                        list[i].VolumeGrowthPct = Math.Round(((decimal)(list[i].TotalAppointments - prevAppointments) / prevAppointments) * 100, 2);
                    }
                }
            }

            return list;
        }
    }
}
