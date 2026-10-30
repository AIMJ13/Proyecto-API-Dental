using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;

namespace WebAPI.DataAccess.Repositories
{
    public class PatientRepository : IPatientRepository
    {
        private readonly string _connectionString;

        public PatientRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private static Patient MapPatient(SqlDataReader reader)
        {
            return new Patient
            {
                PatientId = reader.GetInt32(reader.GetOrdinal("PatientId")),
                FirstName = reader.GetString(reader.GetOrdinal("FirstName")),
                LastName  = reader.GetString(reader.GetOrdinal("LastName")),
                Phone     = reader.IsDBNull(reader.GetOrdinal("Phone"))    ? null : reader.GetString(reader.GetOrdinal("Phone")),
                Address   = reader.IsDBNull(reader.GetOrdinal("Address"))  ? null : reader.GetString(reader.GetOrdinal("Address")),
                BirthDate = reader.IsDBNull(reader.GetOrdinal("BirthDate")) ? "" : reader["BirthDate"].ToString()!,
                State     = reader.GetBoolean(reader.GetOrdinal("IsActive"))
            };
        }


        public async Task<RepositoryResponse<IEnumerable<Patient>>> GetAllAsync()
        {
            var response = new RepositoryResponse<IEnumerable<Patient>>();
            var patients = new List<Patient>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetAllPatient", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    patients.Add(MapPatient(reader));
                reader.Close();

                response.Data = patients;
                response.OperationStatusCode = patients.Count == 0 ? 70015 : 0;
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

        public async Task<RepositoryResponse<PagedResponse<Patient>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null)
        {
            var response =
                new RepositoryResponse<PagedResponse<Patient>>();

            try
            {
                var patients = new List<Patient>();

                using var connection =
                    new SqlConnection(_connectionString);

                await connection.OpenAsync();

                int totalRecords;

                using (var countCommand =
                    new SqlCommand("USP_CountPatients", connection))
                {
                    countCommand.CommandType =
                        CommandType.StoredProcedure;

                    countCommand.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);

                    totalRecords =
                        Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                using (var command =
                    new SqlCommand("USP_GetPatientsPaged", connection))
                {
                    command.CommandType =
                        CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    command.Parameters.AddWithValue("@PageSize", pageSize);
                    command.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                    command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);

                    using var reader =
                        await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        patients.Add(MapPatient(reader));
                    }
                }

                response.Data =
                    new PagedResponse<Patient>
                    {
                        PageNumber = pageNumber,
                        PageSize = pageSize,
                        TotalRecords = totalRecords,
                        TotalPages =
                            (int)Math.Ceiling(
                                totalRecords /
                                (double)pageSize),

                        Data = patients
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

        public async Task<RepositoryResponse<IEnumerable<Patient>>> GetByPhoneAsync(string phone)
        {
            var response = new RepositoryResponse<IEnumerable<Patient>>();
            var patients = new List<Patient>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetPatientByPhone", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@Phone", phone);

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    patients.Add(MapPatient(reader));
                }

                response.Data = patients;
                response.OperationStatusCode = patients.Count == 0 ? 70015 : 0;
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

        public async Task<RepositoryResponse<Patient?>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<Patient?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetPatientById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@PatientId", id);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Patient? patient = null;
                if (await reader.ReadAsync())
                    patient = MapPatient(reader);
                reader.Close();

                response.Data = patient;
                response.OperationStatusCode = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Patient?>> GetByNameAsync(string name)
        {
            var response = new RepositoryResponse<Patient?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetPatientByName", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@FirstName", name);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Patient? patient = null;
                if (await reader.ReadAsync())
                    patient = MapPatient(reader);
                reader.Close();

                response.Data = patient;
                response.OperationStatusCode = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Patient?>> CreateAsync(Patient patient)
        {
            var response = new RepositoryResponse<Patient?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_InsertPatient", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@FirstName", patient.FirstName);
                command.Parameters.AddWithValue("@LastName",  patient.LastName);
                command.Parameters.AddWithValue("@Phone",     (object?)patient.Phone   ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address",   (object?)patient.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@BirthDate", string.IsNullOrEmpty(patient.BirthDate) ? DBNull.Value : (object)DateTime.Parse(patient.BirthDate));
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Patient? created = null;
                if (await reader.ReadAsync())
                    created = MapPatient(reader);
                reader.Close();

                response.Data = created;
                response.OperationStatusCode = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Patient?>> UpdateAsync(Patient patient)
        {
            var response = new RepositoryResponse<Patient?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_UpdatePatient", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@PatientId", patient.PatientId);
                command.Parameters.AddWithValue("@FirstName", patient.FirstName);
                command.Parameters.AddWithValue("@LastName",  patient.LastName);
                command.Parameters.AddWithValue("@Phone",     (object?)patient.Phone   ?? DBNull.Value);
                command.Parameters.AddWithValue("@Address",   (object?)patient.Address ?? DBNull.Value);
                command.Parameters.AddWithValue("@BirthDate", string.IsNullOrEmpty(patient.BirthDate) ? DBNull.Value : (object)DateTime.Parse(patient.BirthDate));
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Patient? updated = null;
                if (await reader.ReadAsync())
                    updated = MapPatient(reader);
                reader.Close();

                response.Data = updated;
                response.OperationStatusCode = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<Patient?>> SetStateAsync(int id, bool state)
        {
            var response = new RepositoryResponse<Patient?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_UpdatePatientState", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@PatientId", id);
                command.Parameters.AddWithValue("@State",     state);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Patient? patient = null;
                if (await reader.ReadAsync())
                    patient = MapPatient(reader);
                reader.Close();

                response.Data = patient;
                response.OperationStatusCode = Convert.ToInt32(command.Parameters["@ReturnValue"].Value);
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
