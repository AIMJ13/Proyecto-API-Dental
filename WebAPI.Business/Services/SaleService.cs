using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.Business.Services
{
    public class SaleService : ISaleService
    {
        private readonly ISaleRepository _saleRepository;

        public SaleService(ISaleRepository saleRepository)
        {
            _saleRepository = saleRepository;
        }

        public async Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto)
        {
            try
            {
                var saleMaster = new Sale
                {
                    AppointmentId = dto.AppointmentId,
                    SaleDate = DateTime.Now.ToString("yyyy-MM-dd")
                };

                var saleDetails = dto.Details.Select(d => new SaleDetail
                {
                    ServiceId = d.ServiceId,
                    Quantity = d.Quantity
                }).ToList();

                var repoResponse = await _saleRepository.InsertAsync(saleMaster, saleDetails);

                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data?.Master != null)
                {
                    var master = repoResponse.Data.Master;
                    var details = repoResponse.Data.Details ?? Enumerable.Empty<SaleDetail>();

                    var dataResponse = new SaleResponseDto
                    {
                        SaleId = master.SaleId,
                        AppointmentId = master.AppointmentId,
                        SaleDate = master.SaleDate,
                        PatientName = master.PatientName,
                        DoctorName = master.DoctorName,
                        Details = details.Select(d => new SaleResponseDetailDto
                        {
                            SaleDetailId = d.SaleDetailId,
                            ServiceId = d.ServiceId,
                            ServiceName = d.ServiceName,
                            Quantity = d.Quantity,
                            Cost = d.Cost
                        }).ToList()
                    };

                    return new ServiceResponse<SaleResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Venta registrada correctamente."
                    };
                }

                return repoResponse.OperationStatusCode switch
                {
                    5050 => new ServiceResponse<SaleResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una venta registrada para esta cita."
                    },
                    5052 => new ServiceResponse<SaleResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "La cita indicada no existe."
                    },
                    _ => new ServiceResponse<SaleResponseDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = repoResponse.Message ?? "Error inesperado al registrar la venta."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<SaleResponseDto>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _saleRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0 && result.Data?.Master != null)
                {
                    var master = result.Data.Master;
                    var details = result.Data.Details ?? Enumerable.Empty<SaleDetail>();

                    var dataResponse = new SaleResponseDto
                    {
                        SaleId = master.SaleId,
                        AppointmentId = master.AppointmentId,
                        SaleDate = master.SaleDate,
                        PatientName = master.PatientName,
                        DoctorName = master.DoctorName,
                        Details = details.Select(d => new SaleResponseDetailDto
                        {
                            SaleDetailId = d.SaleDetailId,
                            ServiceId = d.ServiceId,
                            ServiceName = d.ServiceName,
                            Quantity = d.Quantity,
                            Cost = d.Cost
                        }).ToList()
                    };

                    return new ServiceResponse<SaleResponseDto>
                    {
                        Data = dataResponse,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Venta encontrada correctamente."
                    };
                }

                return new ServiceResponse<SaleResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "No se encontró la venta con el Id proporcionado."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<SaleResponseDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }
        public async Task<ServiceResponse<PagedResponse<Sale>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? patientId = null,
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var result =
                await _saleRepository.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    patientId,
                    doctorId,
                    startDate,
                    endDate);

            return new ServiceResponse<PagedResponse<Sale>>
            {
                Data = result.Data,
                IsSuccess = result.IsSuccess,
                MessageCode = MessageCodes.Success,
                Message = "Consulta exitosa"
            };
        }
        public async Task<ServiceResponse<IEnumerable<SaleResponseDto>>> GetAllAsync()
        {
            try
            {
                var result = await _saleRepository.GetAllAsync();

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    var sales = result.Data.Select(s => new SaleResponseDto
                    {
                        SaleId = s.SaleId,
                        AppointmentId = s.AppointmentId,
                        SaleDate = s.SaleDate,
                        PatientName = s.PatientName,
                        DoctorName = s.DoctorName
                    });

                    return new ServiceResponse<IEnumerable<SaleResponseDto>>
                    {
                        Data = sales,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Operación exitosa."
                    };
                }

                return new ServiceResponse<IEnumerable<SaleResponseDto>>
                {
                    Data = new List<SaleResponseDto>(),
                    IsSuccess = false,
                    MessageCode = MessageCodes.NoData,
                    Message = "No se encontraron ventas registradas."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<IEnumerable<SaleResponseDto>>
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