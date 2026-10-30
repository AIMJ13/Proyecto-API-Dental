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
    public class DoctorService : IDoctorService
    {
        private readonly IDoctorRepository _doctorRepository;

        public DoctorService(IDoctorRepository doctorRepository)
        {
            _doctorRepository = doctorRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Doctor>>> GetAllAsync()
        {
            var result = await _doctorRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
                return new ServiceResponse<IEnumerable<Doctor>>
                {
                    Data = result.Data, IsSuccess = true,
                    MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                70023 => new ServiceResponse<IEnumerable<Doctor>>
                {
                    Data = new List<Doctor>(), IsSuccess = false,
                    MessageCode = MessageCodes.NoData, Message = "No se encontraron registros de doctores."
                },
                _ => new ServiceResponse<IEnumerable<Doctor>>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<(IEnumerable<Doctor> Doctors, int TotalRecords)>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? specialtyId = null,
            bool? isActive = null,
            string? searchTerm = null)
        {
            var result = await _doctorRepository.GetPagedAsync(pageNumber, pageSize, specialtyId, isActive, searchTerm);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<(IEnumerable<Doctor>, int)>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            return new ServiceResponse<(IEnumerable<Doctor>, int)>
            {
                Data = default,
                IsSuccess = false,
                MessageCode = MessageCodes.NoData,
                Message = "No se encontraron registros."
            };
        }

        public async Task<ServiceResponse<Doctor>> GetByIdAsync(int id)
        {
            try
            {
                var result = await _doctorRepository.GetByIdAsync(id);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Doctor>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                    };

                return result.OperationStatusCode switch
                {
                    70024 => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el doctor con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Doctor>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Doctor>> GetByNameAsync(string name)
        {
            var result = await _doctorRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0 && result.Data != null)
                return new ServiceResponse<Doctor>
                {
                    Data = result.Data, IsSuccess = true,
                    MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                70024 => new ServiceResponse<Doctor>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.NotFound, Message = "No se encontró el doctor con ese nombre."
                },
                _ => new ServiceResponse<Doctor>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<Doctor>> CreateAsync(CreateDoctorDto doctorDto)
        {
            try
            {
                var specialtyIds = doctorDto.SpecialtyIds?.ToList() ?? new List<int>();
                if (specialtyIds.Count == 0 && doctorDto.SpecialtyId.HasValue)
                {
                    specialtyIds.Add(doctorDto.SpecialtyId.Value);
                }

                if (specialtyIds.Count == 0)
                {
                    return new ServiceResponse<Doctor>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "Debe especificar al menos una especialidad para el doctor."
                    };
                }

                var doctor = new Doctor
                {
                    FirstName   = doctorDto.FirstName,
                    LastName    = doctorDto.LastName,
                    Phone       = doctorDto.Phone,
                    SpecialtyIds = specialtyIds
                };

                var result = await _doctorRepository.CreateAsync(doctor);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Doctor>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Doctor agregado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    5050 => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.Conflict, Message = "Ya existe un doctor registrado con ese nombre."
                    },
                    5051 => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation, Message = "Una o más especialidades indicadas no existen."
                    },
                    _ => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado al agregar el doctor."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Doctor>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Doctor>> UpdateAsync(int id, UpdateDoctorDto doctorDto)
        {
            try
            {
                var specialtyIds = doctorDto.SpecialtyIds?.ToList() ?? new List<int>();
                if (specialtyIds.Count == 0 && doctorDto.SpecialtyId.HasValue)
                {
                    specialtyIds.Add(doctorDto.SpecialtyId.Value);
                }

                if (specialtyIds.Count == 0)
                {
                    return new ServiceResponse<Doctor>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "Debe especificar al menos una especialidad para el doctor."
                    };
                }

                var doctor = new Doctor
                {
                    DoctorId    = id,
                    FirstName   = doctorDto.FirstName,
                    LastName    = doctorDto.LastName,
                    Phone       = doctorDto.Phone,
                    SpecialtyIds = specialtyIds
                };

                var result = await _doctorRepository.UpdateAsync(doctor);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Doctor>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Doctor actualizado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    70024 => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el doctor con el Id proporcionado."
                    },
                    5051 => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation, Message = "Una o más especialidades indicadas no existen."
                    },
                    _ => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado al actualizar el doctor."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Doctor>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Doctor>> SetStateAsync(int id, bool state)
        {
            try
            {
                var result = await _doctorRepository.SetStateAsync(id, state);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Doctor>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = state ? "Doctor activado correctamente." : "Doctor desactivado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    70024 => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el doctor con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Doctor>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Doctor>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }
    }
}
