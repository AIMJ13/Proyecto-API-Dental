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
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;

        public PatientService(IPatientRepository patientRepository)
        {
            _patientRepository = patientRepository;
        }


        public async Task<ServiceResponse<IEnumerable<Patient>>> GetAllAsync()
        {
            var result = await _patientRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
                return new ServiceResponse<IEnumerable<Patient>>
                {
                    Data = result.Data, IsSuccess = true,
                    MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                70015 => new ServiceResponse<IEnumerable<Patient>>
                {
                    Data = new List<Patient>(), IsSuccess = false,
                    MessageCode = MessageCodes.NoData, Message = "No se encontraron pacientes registrados."
                },
                _ => new ServiceResponse<IEnumerable<Patient>>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<PagedResponse<Patient>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null)
        {
            var result =
                await _patientRepository.GetPagedAsync(
                    pageNumber,
                    pageSize,
                    searchTerm,
                    isActive);

            return new ServiceResponse<PagedResponse<Patient>>
            {
                Data = result.Data,
                IsSuccess = result.IsSuccess,
                Message = "Consulta exitosa",
                MessageCode = MessageCodes.Success
            };
        }

        public async Task<ServiceResponse<IEnumerable<Patient>>> GetByPhoneAsync(string phone)
        {
            var result = await _patientRepository.GetByPhoneAsync(phone);

            if (result.OperationStatusCode == 0 && result.Data != null)
                return new ServiceResponse<IEnumerable<Patient>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };

            return new ServiceResponse<IEnumerable<Patient>>
            {
                Data = new List<Patient>(),
                IsSuccess = false,
                MessageCode = MessageCodes.NotFound,
                Message = "No se encontraron pacientes con ese teléfono."
            };
        }

        public async Task<ServiceResponse<Patient>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _patientRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Patient>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                    };

                return result.OperationStatusCode switch
                {
                    70016 => new ServiceResponse<Patient>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el paciente con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Patient>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Patient>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Patient>> GetByNameAsync(string name)
        {
            var result = await _patientRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0 && result.Data != null)
                return new ServiceResponse<Patient>
                {
                    Data = result.Data, IsSuccess = true,
                    MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                70016 => new ServiceResponse<Patient>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.NotFound, Message = "No se encontró el paciente con ese nombre."
                },
                _ => new ServiceResponse<Patient>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<Patient>> CreateAsync(CreatePatientDto dto)
        {
            try
            {
                var patient = new Patient
                {
                    FirstName = dto.FirstName,
                    LastName  = dto.LastName,
                    Phone     = dto.Phone,
                    Address   = dto.Address,
                    BirthDate = dto.BirthDate ?? ""
                };

                var result = await _patientRepository.CreateAsync(patient);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Patient>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Paciente agregado correctamente."
                    };

                return new ServiceResponse<Patient>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado al agregar el paciente."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Patient>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Patient>> UpdateAsync(int id, UpdatePatientDto dto)
        {
            try
            {
                var patient = new Patient
                {
                    PatientId = id,
                    FirstName = dto.FirstName,
                    LastName  = dto.LastName,
                    Phone     = dto.Phone,
                    Address   = dto.Address,
                    BirthDate = dto.BirthDate ?? ""
                };

                var result = await _patientRepository.UpdateAsync(patient);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Patient>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Paciente actualizado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    70016 => new ServiceResponse<Patient>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el paciente con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Patient>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado al actualizar el paciente."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Patient>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Patient>> SetStateAsync(int id, bool state)
        {
            try
            {
                var result = await _patientRepository.SetStateAsync(id, state);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Patient>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = state ? "Paciente activado correctamente." : "Paciente desactivado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    70016 => new ServiceResponse<Patient>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el paciente con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Patient>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Patient>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }
    }
}
