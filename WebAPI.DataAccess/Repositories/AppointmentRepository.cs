using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.DataAccess.Repositories
{
    public class AppointmentRepository : IAppointmentRepository
    {
        private readonly string _connectionString;

        public AppointmentRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private static Appointment MapAppointment(SqlDataReader reader)
        {
            return new Appointment
            {
                AppointmentId = (int)reader["AppointmentId"],
                Date = reader["Date"].ToString()!,
                Time = reader["Time"].ToString()!,
                Reason = reader["Reason"] == DBNull.Value ? null : reader["Reason"].ToString(),
                PatientId = (int)reader["PatientId"],
                DoctorId = (int)reader["DoctorId"],
                PatientName = reader["PatientName"].ToString(),
                DoctorName = reader["DoctorName"].ToString(),
                Status = reader["Status"] == DBNull.Value ? "Programada" : reader["Status"].ToString()!
            };
        }

        public async Task<RepositoryResponse<IEnumerable<Appointment>>> GetAllAsync()
        {
            var appointments = new List<Appointment>();
            var response = new RepositoryResponse<IEnumerable<Appointment>>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_GetAllAppointment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    appointments.Add(MapAppointment(reader));
                reader.Close();

                response.Data = appointments;
                response.OperationStatusCode = appointments.Count == 0 ? 5051 : 0;
                response.Message = "Exitoso";
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<PagedResponse<Appointment>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? doctorId = null,
            int? patientId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var response =
                new RepositoryResponse<PagedResponse<Appointment>>();

            try
            {
                var appointments =
                    new List<Appointment>();

                using var connection =
                    new SqlConnection(_connectionString);

                await connection.OpenAsync();

                int totalRecords;

                using (var countCommand =
                    new SqlCommand("USP_CountAppointments", connection))
                {
                    countCommand.CommandType =
                        CommandType.StoredProcedure;

                    countCommand.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@PatientId", (object?)patientId ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@StartDate", (object?)startDate?.Date ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@EndDate", (object?)endDate?.Date ?? DBNull.Value);

                    totalRecords =
                        Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                using (var command =
                    new SqlCommand("USP_GetAppointmentsPaged", connection))
                {
                    command.CommandType =
                        CommandType.StoredProcedure;

                    command.Parameters.AddWithValue(
                        "@PageNumber",
                        pageNumber);

                    command.Parameters.AddWithValue(
                        "@PageSize",
                        pageSize);

                    command.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@PatientId", (object?)patientId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@Status", (object?)status ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartDate", (object?)startDate?.Date ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", (object?)endDate?.Date ?? DBNull.Value);

                    using var reader =
                        await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        appointments.Add(MapAppointment(reader));
                    }
                }

                response.Data =
                    new PagedResponse<Appointment>
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages =
                            (int)Math.Ceiling(
                                totalRecords /
                                (double)pageSize),

                        Data = appointments
                    };

                response.OperationStatusCode = 0;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
                response.IsSuccess = false;
            }

            return response;
        }

        public async Task<RepositoryResponse<Appointment?>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<Appointment?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_GetAppointmentById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@AppointmentId", id);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                Appointment? appointment = null;
                if (await reader.ReadAsync())
                    appointment = MapAppointment(reader);
                reader.Close();

                response.Data = appointment;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Appointment?>> CreateAsync(Appointment appointment)
        {
            var response = new RepositoryResponse<Appointment?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_InsertAppointment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Date", DateTime.Parse(appointment.Date));
                cmd.Parameters.AddWithValue("@Time", TimeSpan.Parse(appointment.Time));
                cmd.Parameters.AddWithValue("@Reason", (object?)appointment.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
                cmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                Appointment? created = null;
                if (await reader.ReadAsync())
                    created = MapAppointment(reader);
                reader.Close();

                response.Data = created;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Appointment?>> UpdateAsync(Appointment appointment)
        {
            var response = new RepositoryResponse<Appointment?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_UpdateAppointment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@AppointmentId", appointment.AppointmentId);
                cmd.Parameters.AddWithValue("@Date", DateTime.Parse(appointment.Date));
                cmd.Parameters.AddWithValue("@Time", TimeSpan.Parse(appointment.Time));
                cmd.Parameters.AddWithValue("@Reason", (object?)appointment.Reason ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PatientId", appointment.PatientId);
                cmd.Parameters.AddWithValue("@DoctorId", appointment.DoctorId);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                Appointment? updated = null;
                if (await reader.ReadAsync())
                    updated = MapAppointment(reader);
                reader.Close();

                response.Data = updated;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Appointment>> UpdateStatusAsync(int appointmentId, string status)
        {
            var response = new RepositoryResponse<Appointment>();

            try
            {
                using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("USP_UpdateAppointmentStatus", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@AppointmentId", appointmentId);
                command.Parameters.AddWithValue("@Status", status);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();

                var returnValue = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);

                response.OperationStatusCode = returnValue;

                if (returnValue == 0)
                {
                    var appointmentResponse = await GetByIdAsync(appointmentId);

                    response.Data = appointmentResponse.Data;
                    response.Message = "Solicitud exitosa";
                }
                else if (returnValue == 70002)
                {
                    response.Data = null;
                    response.Message = "No se encontró la cita";
                }
                else if (returnValue == 70060)
                {
                    response.Data = null;
                    response.Message = "Estado de cita no válido";
                }
                else
                {
                    response.Data = null;
                    response.Message = "No se pudo actualizar el estado de la cita";
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<bool>> DeleteAsync(int id)
        {
            var response = new RepositoryResponse<bool>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_DeleteAppointment", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@AppointmentId", id);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                var returnValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                response.Data = returnValue == 0;
                response.OperationStatusCode = returnValue;
            }
            catch (SqlException ex) when (ex.Number == 547)
            {
                response.Data = false;
                response.OperationStatusCode = 5060;
                response.Message = "No se puede eliminar la cita porque tiene una venta registrada.";
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}