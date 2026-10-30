using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.DataAccess.Repositories
{
    public class InvoiceRepository: IInvoiceRepository
    {
        private readonly ConcurrentQueue<Invoice> _invoiceQueue = new ConcurrentQueue<Invoice>();
        private readonly string _connectionString;

        public InvoiceRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }
        public async Task<RepositoryResponse<ConcurrentQueue<Invoice>>> GetInvoiceQueue()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    SqlCommand cmd = new SqlCommand("USP_GetUnPrintedInvoices", connection);
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;

                    using  (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var invoice = new Invoice();

                            invoice.InvoiceId = (int)reader["InvoiceId"];
                            invoice.CreatedDate = (DateTime)reader["CreatedDate"];
                            invoice.SaleId = (int)reader["SaleId"];
                            invoice.IsPrinted = (bool)reader["IsPrinted"];

                            _invoiceQueue.Enqueue(invoice);
                        }
                    }

                    return new RepositoryResponse<ConcurrentQueue<Invoice>>
                    {
                        Data = _invoiceQueue,
                        OperationStatusCode = 0,
                        Message = "Operacion exitosa"
                    };
                }
            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<ConcurrentQueue<Invoice>>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                return new RepositoryResponse<ConcurrentQueue<Invoice>>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }

        public async Task<RepositoryResponse<Invoice>> ToPrint()
        {
            var invoicePrinted = new Invoice();

            try

            {

                if (_invoiceQueue.TryPeek(out Invoice? itemPeek))
                {
                    using (SqlConnection connection = new SqlConnection(_connectionString))
                    {
                        await connection.OpenAsync();

                        SqlCommand cmd = new SqlCommand("USP_ToPrintInvoice", connection);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("InvoiceId", itemPeek.InvoiceId);
                        cmd.Parameters.AddWithValue("Date", DateTime.Now);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var invoice = new Invoice();

                                invoicePrinted.InvoiceId = (int)reader["InvoiceId"];
                                invoicePrinted.CreatedDate = (DateTime)reader["CreatedDate"];
                                invoicePrinted.SaleId = (int)reader["SaleId"];
                                invoicePrinted.IsPrinted = (bool)reader["IsPrinted"];

                            }
                        }
                        return new RepositoryResponse<Invoice>
                        {
                            Data = invoicePrinted,
                            OperationStatusCode = 0,
                            Message = "Operacion exitosa"
                        };
                    }
                }
                else
                {
                    return new RepositoryResponse<Invoice>
                    {
                        Data = null,
                        OperationStatusCode = 2,
                        Message = "No hay elementos en la cola"
                    };
                }


            }
            catch (SqlException ex)
            {
                return new RepositoryResponse<Invoice>
                {
                    Data = null,
                    OperationStatusCode = ex.Number,
                    Message = ex.Message
                };
            }

            catch (Exception ex)
            {
                return new RepositoryResponse<Invoice>
                {
                    Data = null,
                    OperationStatusCode = -1,
                    Message = ex.Message
                };
            }
        }
    }
}
