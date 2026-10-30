using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.DataAccess.Repositories
{
    public class SystemLogRepository : ISystemLogRepository
    {
        private readonly string _connectionString;

        public SystemLogRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        private static SystemLog MapSystemLog(SqlDataReader reader)
        {
            return new SystemLog
            {
                LogId = (int)reader["LogId"],
                Timestamp = (DateTime)reader["Timestamp"],
                UserId = reader["UserId"] == DBNull.Value ? null : (int)reader["UserId"],
                UserName = reader["UserName"].ToString() ?? "Sistema",
                UserRole = reader["UserRole"] == DBNull.Value ? null : reader["UserRole"].ToString(),
                Action = reader["Action"].ToString() ?? string.Empty,
                Module = reader["Module"].ToString() ?? string.Empty,
                EntityId = reader["EntityId"] == DBNull.Value ? null : reader["EntityId"].ToString(),
                Description = reader["Description"].ToString() ?? string.Empty,
                Details = reader["Details"] == DBNull.Value ? null : reader["Details"].ToString(),
                IpAddress = reader["IpAddress"] == DBNull.Value ? null : reader["IpAddress"].ToString(),
                IsSuccess = (bool)reader["IsSuccess"]
            };
        }

        public async Task<RepositoryResponse<int>> InsertAsync(SystemLog log)
        {
            var response = new RepositoryResponse<int>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("dbo.USP_InsertSystemLog", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserId", (object?)log.UserId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserName", string.IsNullOrWhiteSpace(log.UserName) ? "Sistema" : log.UserName);
                cmd.Parameters.AddWithValue("@UserRole", (object?)log.UserRole ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Action", log.Action);
                cmd.Parameters.AddWithValue("@Module", log.Module);
                cmd.Parameters.AddWithValue("@EntityId", (object?)log.EntityId ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Description", log.Description);
                cmd.Parameters.AddWithValue("@Details", (object?)log.Details ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IpAddress", (object?)log.IpAddress ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsSuccess", log.IsSuccess);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    response.Data = Convert.ToInt32(reader["LogId"]);
                    response.OperationStatusCode = 0;
                    response.IsSuccess = true;
                    response.Message = "Log registrado correctamente.";
                }
            }
            catch (Exception ex)
            {
                response.Data = 0;
                response.OperationStatusCode = -1;
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<PagedResponse<SystemLog>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? module = null,
            string? action = null,
            string? userName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            bool? isSuccess = null)
        {
            var response = new RepositoryResponse<PagedResponse<SystemLog>>();
            var logs = new List<SystemLog>();
            int totalRecords = 0;

            pageNumber = Math.Max(1, pageNumber);
            pageSize = Math.Clamp(pageSize, 1, 100);

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("dbo.USP_GetSystemLogsPaged", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@Module", (object?)module ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Action", (object?)action ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@UserName", (object?)userName ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@StartDate", (object?)startDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object?)endDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsSuccess", (object?)isSuccess ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    logs.Add(MapSystemLog(reader));
                    if (totalRecords == 0 && reader["TotalRecords"] != DBNull.Value)
                    {
                        totalRecords = (int)reader["TotalRecords"];
                    }
                }

                response.Data = new PagedResponse<SystemLog>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Data = logs
                };
                response.OperationStatusCode = 0;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<SystemLog?>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<SystemLog?>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("dbo.USP_GetSystemLogById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@LogId", id);

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    response.Data = MapSystemLog(reader);
                    response.OperationStatusCode = 0;
                    response.IsSuccess = true;
                }
                else
                {
                    response.Data = null;
                    response.OperationStatusCode = 404;
                    response.IsSuccess = false;
                    response.Message = "No se encontró el registro de log solicitado.";
                }
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<IEnumerable<string>>> GetModulesAsync()
        {
            var response = new RepositoryResponse<IEnumerable<string>>();
            var modules = new List<string>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("dbo.USP_GetSystemLogModules", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    modules.Add(reader["Module"].ToString() ?? string.Empty);
                }

                response.Data = modules;
                response.OperationStatusCode = 0;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<IEnumerable<string>>> GetActionsAsync()
        {
            var response = new RepositoryResponse<IEnumerable<string>>();
            var actions = new List<string>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("dbo.USP_GetSystemLogActions", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    actions.Add(reader["Action"].ToString() ?? string.Empty);
                }

                response.Data = actions;
                response.OperationStatusCode = 0;
                response.IsSuccess = true;
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.IsSuccess = false;
                response.Message = ex.Message;
            }
            return response;
        }
    }
}
