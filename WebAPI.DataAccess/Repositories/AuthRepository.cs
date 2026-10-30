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
    public class AuthRepository : IAuthRepository
    {
        private readonly string _connectionString;

        public AuthRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<RepositoryResponse<User>> RegisterAsync(User user)
        {
            var userReturned = new User();
            var response = new RepositoryResponse<User>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("USP_RegisterUser", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserName",     user.UserName);
                cmd.Parameters.AddWithValue("@Email",        (object?)user.Email ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PasswordHash", user.PasswordHash!);
                cmd.Parameters.AddWithValue("@IsActive",     user.IsActive);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    userReturned.Id       = (int)reader["Id"];
                    userReturned.UserName = reader["UserName"].ToString()!;
                    userReturned.Email    = reader["Email"].ToString();
                    userReturned.IsActive = (bool)reader["IsActive"];
                }
                reader.Close();

                response.Data = userReturned;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                return response;
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<User> { Data = null, OperationStatusCode = ex.Number, Message = ex.Message };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<User> { Data = null, OperationStatusCode = -1, Message = ex.Message };
            }
        }

        public async Task<RepositoryResponse<User>> GetByUserNameAsync(string name)
        {
            var user = new User();
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
                if (await reader.ReadAsync())
                {
                    user.Id           = (int)reader["Id"];
                    user.UserName     = reader["UserName"].ToString()!;
                    user.PasswordHash = reader["PasswordHash"].ToString();
                    user.Email        = reader["Email"].ToString();
                    user.IsActive     = (bool)reader["IsActive"];
                }
                reader.Close();

                response.Data = user;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                return response;
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<User> { Data = null, OperationStatusCode = ex.Number, Message = ex.Message };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<User> { Data = null, OperationStatusCode = -1, Message = ex.Message };
            }
        }

        public async Task<RepositoryResponse<User>> GetByIdAsync(int id)
        {
            var user = new User();
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
                if (await reader.ReadAsync())
                {
                    user.Id       = (int)reader["Id"];
                    user.UserName = reader["UserName"].ToString()!;
                    user.Email    = reader["Email"].ToString();
                    user.IsActive = (bool)reader["IsActive"];
                }
                reader.Close();

                response.Data = user;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                return response;
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<User> { Data = null, OperationStatusCode = ex.Number, Message = ex.Message };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<User> { Data = null, OperationStatusCode = -1, Message = ex.Message };
            }
        }

        public async Task<RepositoryResponse<User>> GetByEmailAsync(string email)
        {
            var user = new User();
            var response = new RepositoryResponse<User>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("USP_GetUserByEmail", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    user.Id           = (int)reader["Id"];
                    user.UserName     = reader["UserName"].ToString()!;
                    user.PasswordHash = reader["PasswordHash"].ToString()!;
                    user.Email        = reader["Email"].ToString()!;
                    user.IsActive     = (bool)reader["IsActive"];
                }
                reader.Close();

                response.Data = user;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                return response;
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<User> { Data = null, OperationStatusCode = -1, Message = ex.Message };
            }
        }

        public async Task<RepositoryResponse<IEnumerable<string>>> GetRolesByUserIdAsync(int userId)
        {
            var roles = new List<string>();
            var response = new RepositoryResponse<IEnumerable<string>>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("USP_GetUserRolesByUserId", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    roles.Add(reader["RoleName"].ToString()!);
                reader.Close();

                response.Data = roles;
                response.OperationStatusCode = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                response.Message = "Operación exitosa";
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
