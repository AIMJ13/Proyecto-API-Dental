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
    public class ServiceRepository : IServiceRepository
    {
        private readonly string _connectionString;

        public ServiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        private static Service MapService(SqlDataReader reader)
        {
            return new Service
            {
                ServiceId = (int)reader["ServiceId"],
                ServiceName = reader["ServiceName"].ToString() ?? string.Empty,
                Cost = reader["Cost"].ToString() ?? string.Empty,
                State = (bool)reader["IsActive"]
            };
        }

        public async Task<RepositoryResponse<IEnumerable<Service>>> GetAllAsync()
        {
            var response = new RepositoryResponse<IEnumerable<Service>>();
            var services = new List<Service>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetAllService", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    services.Add(MapService(reader));
                reader.Close();

                response.Data = services;
                response.OperationStatusCode = services.Count == 0 ? 70033 : 0;
                response.Message = "Éxito";
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<PagedResponse<Service>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            decimal? minCost = null,
            decimal? maxCost = null,
            bool? isActive = null)
        {
            var response = new RepositoryResponse<PagedResponse<Service>>();

            try
            {
                var services = new List<Service>();
                int totalRecords = 0;

                using var connection = new SqlConnection(_connectionString);

                var cmd = new SqlCommand("USP_GetPagedService", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MinCost", (object?)minCost ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@MaxCost", (object?)maxCost ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);

                await connection.OpenAsync();

                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    services.Add(MapService(reader));
                }

                await reader.NextResultAsync();

                if (await reader.ReadAsync())
                {
                    totalRecords = Convert.ToInt32(reader["TotalRecords"]);
                }

                response.Data = new PagedResponse<Service>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize),
                    Data = services
                };

                response.OperationStatusCode = 0;
            }
            catch (Exception ex)
            {
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<Service?>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<Service?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetServiceById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ServiceId", id);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Service? service = null;
                if (await reader.ReadAsync())
                    service = MapService(reader);
                reader.Close();

                response.Data = service;
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

        public async Task<RepositoryResponse<Service?>> GetByNameAsync(string name)
        {
            var response = new RepositoryResponse<Service?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_GetServiceByName", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ServiceName", name);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Service? service = null;
                if (await reader.ReadAsync())
                    service = MapService(reader);
                reader.Close();

                response.Data = service;
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

        public async Task<RepositoryResponse<Service?>> AddAsync(Service service)
        {
            var response = new RepositoryResponse<Service?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_InsertService", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                command.Parameters.AddWithValue("@Cost", service.Cost);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Service? created = null;
                if (await reader.ReadAsync())
                    created = MapService(reader);
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

        public async Task<RepositoryResponse<Service?>> UpdateAsync(Service service)
        {
            var response = new RepositoryResponse<Service?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var command = new SqlCommand("USP_UpdateService", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                command.Parameters.AddWithValue("@ServiceId", service.ServiceId);
                command.Parameters.AddWithValue("@ServiceName", service.ServiceName);
                command.Parameters.AddWithValue("@Cost", service.Cost);
                command.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await command.ExecuteReaderAsync();
                Service? updated = null;
                if (await reader.ReadAsync())
                    updated = MapService(reader);
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

        public async Task<RepositoryResponse<Service>> SetStateAsync(int id, bool state)
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_UpdateServiceState", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@ServiceId", id);
                cmd.Parameters.AddWithValue("@State", state);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                Service? updated = null;
                if (await reader.ReadAsync())
                    updated = MapService(reader);
                reader.Close();

                return new RepositoryResponse<Service>
                {
                    Data = updated!,
                    OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value)
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<Service>
                {
                    Data = null!,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
