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
    public class RoleRepository : IRoleRepository
    {
        private readonly string _connectionString;

        public RoleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private static Role MapRole(SqlDataReader reader)
        {
            return new Role
            {
                Id       = (int)reader["Id"],
                RoleName = reader["RoleName"].ToString()!,
                State    = (bool)reader["IsActive"]
            };
        }

        public async Task<RepositoryResponse<IEnumerable<Role>>> GetAllAsync()
        {
            var roles = new List<Role>();
            var response = new RepositoryResponse<IEnumerable<Role>>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_GetRoles", connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    roles.Add(MapRole(reader));
                reader.Close();

                response.Data = roles;
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

        public async Task<RepositoryResponse<Role>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<Role>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_GetRoleById", connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@RoleId", id);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                Role? role = null;
                if (await reader.ReadAsync())
                    role = MapRole(reader);
                reader.Close();

                response.Data = role!;
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

        public async Task<RepositoryResponse<Role>> GetByNameAsync(string name)
        {
            var response = new RepositoryResponse<Role>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_GetRolesByName", connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@RoleName", name);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                Role? role = null;
                if (await reader.ReadAsync())
                    role = MapRole(reader);
                reader.Close();

                response.Data = role!;
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

        public async Task<RepositoryResponse<Role>> CreateAsync(Role role)
        {
            var response = new RepositoryResponse<Role>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_InsertRoles", connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                Role? created = null;
                if (await reader.ReadAsync())
                    created = MapRole(reader);
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

        public async Task<RepositoryResponse<Role>> UpdateAsync(Role role)
        {
            var response = new RepositoryResponse<Role>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_UpdateRole", connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@RoleId",   role.Id);
                cmd.Parameters.AddWithValue("@RoleName", role.RoleName);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                Role? updated = null;
                if (await reader.ReadAsync())
                    updated = MapRole(reader);
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

        public async Task<RepositoryResponse<Role>> SetStateAsync(int id, bool state)
        {
            var response = new RepositoryResponse<Role>();
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                var cmd = new SqlCommand("USP_UpdateRoleState", connection) { CommandType = CommandType.StoredProcedure };
                cmd.Parameters.AddWithValue("@RoleId",   id);
                cmd.Parameters.AddWithValue("@IsActive", state);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                Role? role = null;
                if (await reader.ReadAsync())
                    role = MapRole(reader);
                reader.Close();

                response.Data = role!;
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
    }
}
