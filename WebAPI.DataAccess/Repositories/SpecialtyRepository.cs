using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;
using Microsoft.Extensions.Configuration;

namespace WebAPI.DataAccess.Repositories
{
    public class SpecialtyRepository : ISpecialtyRepository
    {
        private readonly string _connectionString;

        public SpecialtyRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        private static Specialty MapSpecialty(SqlDataReader reader)
        {
            return new Specialty
            {
                SpecialtyId = (int)reader["SpecialtyId"],
                SpecialtyName = reader["SpecialtyName"].ToString() ?? string.Empty,
                State = Convert.ToBoolean(reader["State"])
            };
        }

        public async Task<RepositoryResponse<IEnumerable<Specialty>>> GetAllAsync()
        {
            var response = new RepositoryResponse<IEnumerable<Specialty>>();
            var specialties = new List<Specialty>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetAllSpecialty", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    specialties.Add(MapSpecialty(reader));
                reader.Close();

                response.Data = specialties;
                response.OperationStatusCode = specialties.Count == 0 ? 70001 : 0;
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

        public async Task<RepositoryResponse<PagedResponse<Specialty>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null)
        {
            var response = new RepositoryResponse<PagedResponse<Specialty>>();

            try
            {
                var specialties = new List<Specialty>();
                int totalRecords = 0;

                using var connection = new SqlConnection(_connectionString);

                var command = new SqlCommand("USP_GetPagedSpecialty", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.AddWithValue("@PageNumber", pageNumber);
                command.Parameters.AddWithValue("@PageSize", pageSize);
                command.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                command.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();

                using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    specialties.Add(MapSpecialty(reader));
                    totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }

                reader.Close();

                response.Data = new PagedResponse<Specialty>
                {
                    Data = specialties,
                    TotalRecords = totalRecords,
                    PageNumber = pageNumber,
                    PageSize = pageSize
                };

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

        public async Task<RepositoryResponse<Specialty?>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<Specialty?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetSpecialtyById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@SpecialtyId", id);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Specialty? specialty = null;
                if (await reader.ReadAsync())
                    specialty = MapSpecialty(reader);
                reader.Close();

                response.Data = specialty;
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

        public async Task<RepositoryResponse<Specialty?>> GetByNameAsync(string name)
        {
            var response = new RepositoryResponse<Specialty?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetSpecialtyByName", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@SpecialtyName", name);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Specialty? specialty = null;
                if (await reader.ReadAsync())
                    specialty = MapSpecialty(reader);
                reader.Close();

                response.Data = specialty;
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

        public async Task<RepositoryResponse<Specialty?>> AddAsync(Specialty specialty)
        {
            var response = new RepositoryResponse<Specialty?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_InsertSpecialty", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@SpecialtyName", specialty.SpecialtyName);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Specialty? created = null;
                if (await reader.ReadAsync())
                    created = MapSpecialty(reader);
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

        public async Task<RepositoryResponse<Specialty?>> UpdateAsync(Specialty specialty)
        {
            var response = new RepositoryResponse<Specialty?>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_UpdateSpecialty", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@SpecialtyId", specialty.SpecialtyId);
                command.Parameters.AddWithValue("@SpecialtyName", specialty.SpecialtyName);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Specialty? updated = null;
                if (await reader.ReadAsync())
                    updated = MapSpecialty(reader);
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

        public async Task<RepositoryResponse<Specialty>> SetStateAsync(int id, bool state)
        {
            var response = new RepositoryResponse<Specialty>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_UpdateSpecialtyState", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@SpecialtyId", id);
                cmd.Parameters.AddWithValue("@State", state);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();

                Specialty? updated = null;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        updated = MapSpecialty(reader);
                    }
                }

                var operationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                if (operationStatusCode == 0 && updated == null)
                {
                    var getByIdResponse = await GetByIdAsync(id);
                    updated = getByIdResponse.Data;
                }

                response.Data = updated!;
                response.OperationStatusCode = operationStatusCode;
                response.Message = updated != null
                    ? "Estado actualizado correctamente"
                    : "No se recibió la especialidad actualizada";

                return response;
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Specialty>
                {
                    Data = null!,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
