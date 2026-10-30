using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using WebAPI.DataAccess.Interfaces;
using WebAPI.Core.Entities;
using Microsoft.Data.SqlClient;
using WebAPI.Core.Common;
using Microsoft.Extensions.Configuration;

namespace WebAPI.DataAccess.Repositories
{
    public class DoctorRepository : IDoctorRepository
    {
        private readonly string _connectionString;

        public DoctorRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        private static Doctor MapDoctor(SqlDataReader reader)
        {
            var specialtiesJson = reader["SpecialtiesJson"]?.ToString();
            var specialties = new List<DoctorSpecialtyInfo>();
            if (!string.IsNullOrWhiteSpace(specialtiesJson))
            {
                try
                {
                    specialties = JsonSerializer.Deserialize<List<DoctorSpecialtyInfo>>(
                        specialtiesJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<DoctorSpecialtyInfo>();
                }
                catch
                {
                    specialties = new List<DoctorSpecialtyInfo>();
                }
            }

            var doctor = new Doctor
            {
                DoctorId = reader.GetInt32(reader.GetOrdinal("DoctorId")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName = reader.GetString(reader.GetOrdinal("LastName")),
                Phone = reader.IsDBNull(reader.GetOrdinal("Phone")) ? null : reader.GetString(reader.GetOrdinal("Phone")),
                SpecialtyName = reader.IsDBNull(reader.GetOrdinal("SpecialtyName")) ? null : reader["SpecialtyName"].ToString(),
                Specialties = specialties,
                SpecialtyIds = specialties.Select(s => s.SpecialtyId).ToList(),
                IsActive = reader.GetBoolean(reader.GetOrdinal("IsActive")),
                State = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };

            return doctor;
        }

        public async Task<RepositoryResponse<IEnumerable<Doctor>>> GetAllAsync()
        {
            var response = new RepositoryResponse<IEnumerable<Doctor>>();
            var doctors = new List<Doctor>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetAllDoctor", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    doctors.Add(MapDoctor(reader));
                }
                reader.Close();

                var returnValue = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
                response.Data = doctors;
                response.OperationStatusCode = doctors.Count == 0 ? 70023 : 0;
                response.Message = "Solicitud exitosa";
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<(IEnumerable<Doctor> Doctors, int TotalRecords)>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? specialtyId = null,
            bool? isActive = null,
            string? searchTerm = null)
        {
            var response = new RepositoryResponse<(IEnumerable<Doctor>, int)>();
            var doctors = new List<Doctor>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetDoctorsPaged", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@PageNumber", pageNumber);
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.Parameters.AddWithValue("@SpecialtyId", (object?)specialtyId ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);
                command.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);

                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();
                int totalRecords = 0;

                while (await reader.ReadAsync())
                {
                    doctors.Add(MapDoctor(reader));

                    if (totalRecords == 0)
                        totalRecords = (int)reader["TotalRecords"];
                }

                response.Data = (doctors, totalRecords);
                response.OperationStatusCode = doctors.Any() ? 0 : 70023;
            }
            catch (Exception ex)
            {
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<Doctor?>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<Doctor?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetDoctorById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@DoctorId", id);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Doctor? doctor = null;
                if (await reader.ReadAsync())
                {
                    doctor = MapDoctor(reader);
                }
                reader.Close();

                var returnValue = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
                response.Data = doctor;
                response.OperationStatusCode = returnValue;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<Doctor?>> GetByNameAsync(string name)
        {
            var response = new RepositoryResponse<Doctor?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetDoctorByName", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@FirstName", name);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Doctor? doctor = null;
                if (await reader.ReadAsync())
                {
                    doctor = MapDoctor(reader);
                }
                reader.Close();

                var returnValue = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
                response.Data = doctor;
                response.OperationStatusCode = returnValue;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<Doctor?>> CreateAsync(Doctor doctor)
        {
            var response = new RepositoryResponse<Doctor?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_InsertDoctor", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@FirstName",   doctor.FirstName);
                command.Parameters.AddWithValue("@LastName",    doctor.LastName);
                command.Parameters.AddWithValue("@Phone",       (object?)doctor.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@SpecialtyIds", string.Join(",", doctor.SpecialtyIds));
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Doctor? created = null;
                if (await reader.ReadAsync())
                {
                    created = MapDoctor(reader);
                }
                reader.Close();

                var returnValue = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
                response.Data = created;
                response.OperationStatusCode = returnValue;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<Doctor?>> UpdateAsync(Doctor doctor)
        {
            var response = new RepositoryResponse<Doctor?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_UpdateDoctor", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@DoctorId",    doctor.DoctorId);
                command.Parameters.AddWithValue("@FirstName",   doctor.FirstName);
                command.Parameters.AddWithValue("@LastName",    doctor.LastName);
                command.Parameters.AddWithValue("@Phone",       (object?)doctor.Phone ?? DBNull.Value);
                command.Parameters.AddWithValue("@SpecialtyIds", string.Join(",", doctor.SpecialtyIds));
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Doctor? updated = null;
                if (await reader.ReadAsync())
                {
                    updated = MapDoctor(reader);
                }
                reader.Close();

                var returnValue = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
                response.Data = updated;
                response.OperationStatusCode = returnValue;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<Doctor?>> SetStateAsync(int id, bool state)
        {
            var response = new RepositoryResponse<Doctor?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_UpdateDoctorState", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@DoctorId", id);
                command.Parameters.AddWithValue("@State",    state);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Doctor? doctor = null;
                if (await reader.ReadAsync())
                {
                    doctor = MapDoctor(reader);
                }
                reader.Close();

                var returnValue = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
                response.Data = doctor;
                response.OperationStatusCode = returnValue;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}