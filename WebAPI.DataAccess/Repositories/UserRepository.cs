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
    public class UserRepository : IUserRepository
    {
        private readonly string _connectionString;

        public UserRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<IEnumerable<User>>> GetAllAsync()
        {
            var users = new List<User>();
            var response = new RepositoryResponse<IEnumerable<User>>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_GetAllUsers", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var rolesStr = reader["Roles"]?.ToString();
                    var rolesList = string.IsNullOrWhiteSpace(rolesStr)
                        ? new List<string>()
                        : rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                    users.Add(new User
                    {
                        Id = (int)reader["Id"],
                        UserName = reader["UserName"].ToString()!,
                        Email = reader["Email"]?.ToString(),
                        IsActive = (bool)reader["IsActive"],
                        Roles = rolesList
                    });
                }
                reader.Close();

                response.Data = users;
                response.OperationStatusCode = users.Count == 0 ? 5053 : 0;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<PagedResponse<User>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null)
        {
            var response = new RepositoryResponse<PagedResponse<User>>();
            var users = new List<User>();
            int totalRecords = 0;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_GetUsersPaged", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@PageNumber", pageNumber);
                cmd.Parameters.AddWithValue("@PageSize", pageSize);
                cmd.Parameters.AddWithValue("@SearchTerm", (object?)searchTerm ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@IsActive", (object?)isActive ?? DBNull.Value);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var rolesStr = reader["Roles"]?.ToString();
                    var rolesList = string.IsNullOrWhiteSpace(rolesStr)
                        ? new List<string>()
                        : rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                    users.Add(new User
                    {
                        Id = (int)reader["Id"],
                        UserName = reader["UserName"].ToString()!,
                        Email = reader["Email"].ToString(),
                        IsActive = (bool)reader["IsActive"],
                        Roles = rolesList
                    });

                    if (totalRecords == 0)
                        totalRecords = (int)reader["TotalRecords"];
                }
                reader.Close();

                response.Data = new PagedResponse<User>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Data = users
                };
                response.OperationStatusCode = users.Count == 0 ? 5053 : 0;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<User>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<User>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_GetUserById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                User? user = null;
                if (await reader.ReadAsync())
                {
                    var rolesStr = reader["Roles"]?.ToString();
                    var rolesList = string.IsNullOrWhiteSpace(rolesStr)
                        ? new List<string>()
                        : rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                    user = new User
                    {
                        Id = (int)reader["Id"],
                        UserName = reader["UserName"].ToString()!,
                        PasswordHash = reader["PasswordHash"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        IsActive = (bool)reader["IsActive"],
                        Roles = rolesList
                    };
                }
                reader.Close();

                response.Data = user!;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<User>> GetByNameAsync(string name)
        {
            var response = new RepositoryResponse<User>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_GetUsersByName", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                User? user = null;
                if (await reader.ReadAsync())
                {
                    var rolesStr = reader["Roles"]?.ToString();
                    var rolesList = string.IsNullOrWhiteSpace(rolesStr)
                        ? new List<string>()
                        : rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                    user = new User
                    {
                        Id = (int)reader["Id"],
                        UserName = reader["UserName"].ToString()!,
                        PasswordHash = reader["PasswordHash"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        IsActive = (bool)reader["IsActive"],
                        Roles = rolesList
                    };
                }
                reader.Close();

                response.Data = user!;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<User>> AddAsync(User user)
        {
            var response = new RepositoryResponse<User>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_RegisterUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserName", user.UserName);
                cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash!);
                cmd.Parameters.AddWithValue("@IsActive", user.IsActive);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                User? created = null;
                if (await reader.ReadAsync())
                {
                    created = new User
                    {
                        Id = (int)reader["Id"],
                        UserName = reader["UserName"].ToString()!,
                        Email = reader["Email"].ToString(),
                        IsActive = (bool)reader["IsActive"]
                    };
                }
                reader.Close();

                response.Data = created!;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<User>> UpdateAsync(int id, User user)
        {
            var response = new RepositoryResponse<User>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_UpdateUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", id);
                cmd.Parameters.AddWithValue("@UserName", user.UserName);
                cmd.Parameters.AddWithValue("@Email", (object?)user.Email ?? DBNull.Value);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                User? updated = null;
                if (await reader.ReadAsync())
                {
                    var rolesStr = reader["Roles"]?.ToString();
                    var rolesList = string.IsNullOrWhiteSpace(rolesStr)
                        ? new List<string>()
                        : rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                    updated = new User
                    {
                        Id = (int)reader["Id"],
                        UserName = reader["UserName"].ToString()!,
                        Email = reader["Email"]?.ToString(),
                        IsActive = (bool)reader["IsActive"],
                        Roles = rolesList
                    };
                }
                reader.Close();

                response.Data = updated!;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<User>> SetStateAsync(int userId, bool state)
        {
            var response = new RepositoryResponse<User>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_UpdateUserState", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@IsActive", state);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                User? updated = null;
                if (await reader.ReadAsync())
                {
                    var rolesStr = reader["Roles"]?.ToString();
                    var rolesList = string.IsNullOrWhiteSpace(rolesStr)
                        ? new List<string>()
                        : rolesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();

                    updated = new User
                    {
                        Id = (int)reader["Id"],
                        UserName = reader["UserName"].ToString()!,
                        Email = reader["Email"]?.ToString(),
                        IsActive = (bool)reader["IsActive"],
                        Roles = rolesList
                    };
                }
                reader.Close();

                response.Data = updated!;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<UserRole>> AssignUserRoleAsync(int userId, int roleId)
        {
            var response = new RepositoryResponse<UserRole>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_AssignRoleToUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RoleId", roleId);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await cmd.ExecuteNonQueryAsync();

                response.Data = new UserRole { UserId = userId, RoleId = roleId };
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
            }
            catch (Exception ex)
            {
                response.Data = null!;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<bool>> UpdatePasswordAsync(int userId, string passwordHash)
        {
            var response = new RepositoryResponse<bool>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_UpdateUserPassword", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await cmd.ExecuteNonQueryAsync();

                int statusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                response.OperationStatusCode = statusCode;
                response.Data = (statusCode == 0);
            }
            catch (Exception ex)
            {
                response.Data = false;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }
            return response;
        }

        public async Task<RepositoryResponse<bool>> SyncUserRolesAsync(int userId, IEnumerable<int> roleIds)
        {
            var response = new RepositoryResponse<bool>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_SyncUserRoles", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RoleIds", string.Join(",", roleIds));
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await cmd.ExecuteNonQueryAsync();

                int statusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                response.OperationStatusCode = statusCode;
                response.Data = (statusCode == 0);
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