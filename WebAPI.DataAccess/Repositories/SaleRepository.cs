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
    public class SaleRepository : ISaleRepository
    {
        private readonly string _connectionString;

        public SaleRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        private static Sale MapSale(SqlDataReader reader)
        {
            return new Sale
            {
                SaleId = Convert.ToInt32(reader["SaleId"]),
                AppointmentId = Convert.ToInt32(reader["AppointmentId"]),
                SaleDate = reader["SaleDate"].ToString() ?? string.Empty,
                PatientId = Convert.ToInt32(reader["PatientId"]),
                DoctorId = Convert.ToInt32(reader["DoctorId"]),
                PatientName = reader["PatientName"].ToString(),
                DoctorName = reader["DoctorName"].ToString(),

                TotalAmount = HasColumn(reader, "TotalAmount") && reader["TotalAmount"] != DBNull.Value
                    ? Convert.ToDecimal(reader["TotalAmount"])
                    : 0,

                ServicesCount = HasColumn(reader, "ServicesCount") && reader["ServicesCount"] != DBNull.Value
                    ? Convert.ToInt32(reader["ServicesCount"])
                    : 0
            };
        }

        private static SaleDetail MapDetail(SqlDataReader reader)
        {
            return new SaleDetail
            {
                SaleDetailId = (int)reader["SaleDetailId"],
                SaleId = (int)reader["SaleId"],
                ServiceId = (int)reader["ServiceId"],
                Quantity = (int)reader["Quantity"],
                ServiceName = reader["ServiceName"].ToString(),
                Cost = reader["Cost"].ToString()
            };
        }
        private async Task<List<SaleDetail>> GetDetailsBySaleIdAsync(int saleId)
        {
            var details = new List<SaleDetail>();

            using var connection = new SqlConnection(_connectionString);

            var command = new SqlCommand("USP_GetSaleDetailsBySaleId", connection)
            {
                CommandType = CommandType.StoredProcedure
            };

            command.Parameters.AddWithValue("@SaleId", saleId);

            await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                details.Add(MapDetail(reader));
            }

            return details;
        }
        public async Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetail> details)
        {
            var response = new RepositoryResponse<SaleTransaction>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var cmd = new SqlCommand("USP_InsertSale", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@AppointmentId", master.AppointmentId);
                cmd.Parameters.AddWithValue("@SaleDate", DateTime.Parse(master.SaleDate));
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                using var reader = await cmd.ExecuteReaderAsync();
                Sale? sale = null;
                if (await reader.ReadAsync())
                    sale = MapSale(reader);
                reader.Close();

                var returnValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);
                if (returnValue != 0 || sale == null)
                {
                    response.Data = null;
                    response.OperationStatusCode = returnValue;
                    response.Message = returnValue == 5050 ? "Ya existe una venta para esta cita." :
                                       returnValue == 5052 ? "La cita indicada no existe." : "Error al registrar la venta.";
                    return response;
                }

                var detailsList = new List<SaleDetail>();
                foreach (var detail in details)
                {
                    using var detailConnection = new SqlConnection(_connectionString);
                    await detailConnection.OpenAsync();

                    var detailCmd = new SqlCommand("USP_InsertSaleDetail", detailConnection)
                    {
                        CommandType = CommandType.StoredProcedure
                    };
                    detailCmd.Parameters.AddWithValue("@SaleId", sale.SaleId);
                    detailCmd.Parameters.AddWithValue("@ServiceId", detail.ServiceId);
                    detailCmd.Parameters.AddWithValue("@Quantity", detail.Quantity);
                    detailCmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                   
                    using var detailReader = await detailCmd.ExecuteReaderAsync();
                    if (await detailReader.ReadAsync())
                        detailsList.Add(MapDetail(detailReader));
                    await detailReader.CloseAsync();
                   
                }

                sale.Details = detailsList;

                response.Data = new SaleTransaction { Master = sale, Details = detailsList };
                response.OperationStatusCode = 0;
                response.Message = "Venta registrada correctamente.";
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<SaleTransaction>> GetByIdAsync(int id)
        {
            var response = new RepositoryResponse<SaleTransaction>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_GetSaleById", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.AddWithValue("@SaleId", id);
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();

                Sale? sale = null;
                if (await reader.ReadAsync())
                    sale = MapSale(reader);

                await reader.NextResultAsync();

                var details = new List<SaleDetail>();
                while (await reader.ReadAsync())
                    details.Add(MapDetail(reader));
                reader.Close();

                var returnValue = Convert.ToInt32(cmd.Parameters["@ReturnValue"].Value);

                if (sale == null)
                {
                    response.Data = null;
                    response.OperationStatusCode = 5051;
                    return response;
                }

                sale.Details = details;
                response.Data = new SaleTransaction { Master = sale, Details = details };
                response.OperationStatusCode = 0;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        public async Task<RepositoryResponse<PagedResponse<Sale>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? patientId = null,
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var response = new RepositoryResponse<PagedResponse<Sale>>();

            try
            {
                var sales = new List<Sale>();

                using var connection = new SqlConnection(_connectionString);

                await connection.OpenAsync();

                int totalRecords;

                using (var countCommand = new SqlCommand("USP_CountSales", connection))
                {
                    countCommand.CommandType = CommandType.StoredProcedure;

                    countCommand.Parameters.AddWithValue("@PatientId", (object?)patientId ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@StartDate", (object?)startDate?.Date ?? DBNull.Value);
                    countCommand.Parameters.AddWithValue("@EndDate", (object?)endDate?.Date ?? DBNull.Value);

                    totalRecords = Convert.ToInt32(await countCommand.ExecuteScalarAsync());
                }

                using (var command = new SqlCommand("USP_GetSalesPaged", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@PageNumber", pageNumber);
                    command.Parameters.AddWithValue("@PageSize", pageSize);
                    command.Parameters.AddWithValue("@PatientId", (object?)patientId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@DoctorId", (object?)doctorId ?? DBNull.Value);
                    command.Parameters.AddWithValue("@StartDate", (object?)startDate?.Date ?? DBNull.Value);
                    command.Parameters.AddWithValue("@EndDate", (object?)endDate?.Date ?? DBNull.Value);

                    using var reader = await command.ExecuteReaderAsync();

                    while (await reader.ReadAsync())
                    {
                        sales.Add(MapSale(reader));
                    }
                }

                foreach (var sale in sales)
                {
                    sale.Details = await GetDetailsBySaleIdAsync(sale.SaleId);
                }

                response.Data = new PagedResponse<Sale>
                {
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalRecords = totalRecords,
                    TotalPages = (int)Math.Ceiling(totalRecords / (double)pageSize),
                    Data = sales
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

        public async Task<RepositoryResponse<IEnumerable<Sale>>> GetAllAsync()
        {
            var sales = new List<Sale>();
            var response = new RepositoryResponse<IEnumerable<Sale>>();

            try
            {
                using var connection = new SqlConnection(_connectionString);
                var cmd = new SqlCommand("USP_GetAllSales", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };
                cmd.Parameters.Add("@ReturnValue", SqlDbType.Int).Direction = ParameterDirection.ReturnValue;

                await connection.OpenAsync();
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                    sales.Add(MapSale(reader));
                reader.Close();

                response.Data = sales;
                response.OperationStatusCode = sales.Count == 0 ? 5051 : 0;
            }
            catch (Exception ex)
            {
                response.Data = null;
                response.OperationStatusCode = -1;
                response.Message = ex.Message;
            }

            return response;
        }

        private static bool HasColumn(SqlDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
            {
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }
    }
}