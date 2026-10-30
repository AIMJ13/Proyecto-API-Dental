using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.Business.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IInvoiceRepository _invoiceRepository;

        public InvoiceService(IInvoiceRepository invoiceRepository)
        {
            _invoiceRepository = invoiceRepository;
        }
        public async Task<ServiceResponse<List<Invoice>>> InvoiceQueue()
        {
            try
            {
                var repoResponse = await _invoiceRepository.GetInvoiceQueue();

                if (repoResponse.OperationStatusCode == 0)
                {
                    if (repoResponse.Data != null && repoResponse.Data.Count > 0)
                    {
                        return new ServiceResponse<List<Invoice>>
                        {
                            Data = repoResponse.Data!.ToList(),
                            IsSuccess = true,
                            MessageCode = MessageCodes.Success,
                            Message = "Operacion exitosa"
                        };
                    }
                    else
                    {
                        return new ServiceResponse<List<Invoice>>
                        {
                            Data = new List<Invoice>(),
                            IsSuccess = false,
                            MessageCode = MessageCodes.NoData,
                            Message = "No hay facturas pendientes de imprimir."
                        };
                    }
                }

                return new ServiceResponse<List<Invoice>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<List<Invoice>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
        }

        public async Task<ServiceResponse<Invoice>> ToPrint()
        {
            try
            {
                var getInvoiceQueue = await _invoiceRepository.GetInvoiceQueue();
                if(getInvoiceQueue.Data!.Count<1)
                {
                    return new ServiceResponse<Invoice>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "No hay facturas por imprimir"
                    };
                }
                var repoResponse = await _invoiceRepository.ToPrint();
                if(repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Invoice>
                    {
                        Data = repoResponse.Data!,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Factura impresa"
                    };
                }
                if (repoResponse.OperationStatusCode == 2)
                {
                    return new ServiceResponse<Invoice>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NoData,
                        Message = "No hay facturas por imprimir"
                    };
                }
                return new ServiceResponse<Invoice>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Invoice>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }
            
        
    }
}
