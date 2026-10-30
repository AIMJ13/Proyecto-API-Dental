using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.Business.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;

        public AppointmentService(IAppointmentRepository appointmentRepository)
        {
            _appointmentRepository = appointmentRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Appointment>>> GetAllAsync()
        {
            var result = await _appointmentRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
                return new ServiceResponse<IEnumerable<Appointment>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                5051 => new ServiceResponse<IEnumerable<Appointment>>
                {
                    Data = new List<Appointment>(),
                    IsSuccess = false,
                    MessageCode = MessageCodes.NoData,
                    Message = "No se encontraron citas registradas."
                },
                _ => new ServiceResponse<IEnumerable<Appointment>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error inesperado."
                }
            };
        }
        public async Task<ServiceResponse<PagedResponse<Appointment>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? doctorId = null,
            int? patientId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var result =
                await _appointmentRepository.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    doctorId,
                    patientId,
                    status,
                    startDate,
                    endDate);

            return new ServiceResponse<PagedResponse<Appointment>>
            {
                Data = result.Data,
                IsSuccess = result.IsSuccess,
                Message = "Consulta exitosa",
                MessageCode = MessageCodes.Success
            };
        }

        public async Task<ServiceResponse<Appointment>> GetByIdAsync(int id)
        {
            var result = await _appointmentRepository.GetByIdAsync(id);

            if (result.OperationStatusCode == 0 && result.Data != null)
                return new ServiceResponse<Appointment>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                5051 => new ServiceResponse<Appointment>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "No se encontró la cita con el Id proporcionado."
                },
                _ => new ServiceResponse<Appointment>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<Appointment>> CreateAsync(CreateAppointmentDto dto)
        {
            try
            {
                var appointment = new Appointment
                {
                    Date = dto.Date,
                    Time = dto.Time,
                    Reason = dto.Reason,
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId
                };

                var result = await _appointmentRepository.CreateAsync(appointment);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Appointment>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Cita agendada correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    5052 => new ServiceResponse<Appointment>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "El paciente indicado no existe."
                    },
                    5053 => new ServiceResponse<Appointment>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "El doctor indicado no existe."
                    },
                    5054 => new ServiceResponse<Appointment>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "El doctor ya tiene una cita agendada en la misma fecha y hora."
                    },
                    _ => new ServiceResponse<Appointment>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Appointment>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Appointment>> UpdateAsync(int id, UpdateAppointmentDto dto)
        {
            try
            {
                var appointment = new Appointment
                {
                    AppointmentId = id,
                    Date = dto.Date,
                    Time = dto.Time,
                    Reason = dto.Reason,
                    PatientId = dto.PatientId,
                    DoctorId = dto.DoctorId
                };

                var result = await _appointmentRepository.UpdateAsync(appointment);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Appointment>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Cita actualizada correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    5051 => new ServiceResponse<Appointment>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontró la cita con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Appointment>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Appointment>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }
        public async Task<ServiceResponse<Appointment>> UpdateStatusAsync(int appointmentId, string status)
        {
            var serviceResponse = new ServiceResponse<Appointment>();

            var repositoryResponse = await _appointmentRepository.UpdateStatusAsync(appointmentId, status);

            serviceResponse.Data = repositoryResponse.Data;
            serviceResponse.Message = repositoryResponse.Message;

            switch (repositoryResponse.OperationStatusCode)
            {
                case 0:
                    serviceResponse.MessageCode = MessageCodes.Success;
                    serviceResponse.Message = "Estado de cita actualizado correctamente.";
                    break;

                case 70002:
                    serviceResponse.MessageCode = MessageCodes.NotFound;
                    serviceResponse.Message = "No se encontró la cita.";
                    break;

                case 70060:
                    serviceResponse.MessageCode = MessageCodes.ErrorDataBase;
                    serviceResponse.Message = "Estado de cita no válido.";
                    break;

                default:
                    serviceResponse.MessageCode = MessageCodes.ErrorDataBase;
                    serviceResponse.Message = repositoryResponse.Message;
                    break;
            }

            return serviceResponse;
        }

        public async Task<ServiceResponse<bool>> DeleteAsync(int id)
        {
            try
            {
                var result = await _appointmentRepository.DeleteAsync(id);

                if (result.OperationStatusCode == 0)
                    return new ServiceResponse<bool>
                    {
                        Data = true,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Cita eliminada correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    5051 => new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No se encontró la cita con el Id proporcionado."
                    },

                    5060 => new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "No se puede eliminar la cita porque tiene una venta registrada."
                    },
                    _ => new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }

    }

}

     
