using DentalHouseWebAPI.DTOs;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.Business.Services
{
    public class SpecialtyService : ISpecialtyService
    {
        private readonly ISpecialtyRepository _specialtyRepository;

        public SpecialtyService(ISpecialtyRepository specialtyRepository)
        {
            _specialtyRepository = specialtyRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Specialty>>> GetAllAsync()
        {
            var result = await _specialtyRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Specialty>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            if (result.OperationStatusCode == 70001)
            {
                return new ServiceResponse<IEnumerable<Specialty>>
                {
                    Data = Enumerable.Empty<Specialty>(),
                    IsSuccess = false,
                    MessageCode = MessageCodes.NoData,
                    Message = "No se encontraron registros"
                };
            }

            return new ServiceResponse<IEnumerable<Specialty>>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = result.Message ?? "Ocurrió un error inesperado"
            };
        }

        public async Task<ServiceResponse<PagedResponse<Specialty>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null)
        {
            var result = await _specialtyRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, isActive);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<Specialty>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            if (result.OperationStatusCode == 70001)
            {
                return new ServiceResponse<PagedResponse<Specialty>>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NoData,
                    Message = "No se encontraron registros"
                };
            }

            return new ServiceResponse<PagedResponse<Specialty>>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = result.Message ?? "Ocurrió un error inesperado"
            };
        }

        public async Task<ServiceResponse<Specialty>> GetByIdAsync(int id)
        {
            var repoResponse = await _specialtyRepository.GetByIdAsync(id);

            if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = repoResponse.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            if (repoResponse.OperationStatusCode == 70003)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "No se encontró la Especialidad con el Id proporcionado"
                };
            }

            return new ServiceResponse<Specialty>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = repoResponse.Message ?? "Ocurrió un error inesperado"
            };
        }

        public async Task<ServiceResponse<Specialty>> GetByNameAsync(string name)
        {
            var result = await _specialtyRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0 && result.Data != null)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operación exitosa"
                };
            }

            if (result.OperationStatusCode == 70003)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "No se encontró Especialidad con ese nombre"
                };
            }

            return new ServiceResponse<Specialty>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = result.Message ?? "Error en la base de datos al obtener la Especialidad"
            };
        }

        public async Task<ServiceResponse<Specialty>> CreateAsync(CreateSpecialtyDto newSpecialty)
        {
            try
            {
                var existingSpecialty = await _specialtyRepository.GetByNameAsync(newSpecialty.SpecialtyName);

                if (existingSpecialty.OperationStatusCode == 0 && existingSpecialty.Data != null)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                var specialty = new Specialty
                {
                    SpecialtyName = newSpecialty.SpecialtyName,
                    State = newSpecialty.State
                };

                var result = await _specialtyRepository.AddAsync(specialty);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Registro creado con éxito"
                    };
                }

                if (result.OperationStatusCode == 5050)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una Especialidad con ese nombre"
                    };
                }

                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Ocurrió un error inesperado"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Specialty>> UpdateAsync(int id, UpdateSpecialtyDto specialty)
        {
            try
            {
                var existingById = await _specialtyRepository.GetByIdAsync(id);

                if (existingById.OperationStatusCode == 70003 || existingById.Data == null)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe una Especialidad asociada al Id proporcionado"
                    };
                }

                var existingByName = await _specialtyRepository.GetByNameAsync(specialty.SpecialtyName);

                if (existingByName.OperationStatusCode == 0 &&
                    existingByName.Data != null &&
                    existingByName.Data.SpecialtyId != id)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una Especialidad con el nombre proporcionado"
                    };
                }

                var dataSpecialty = new Specialty
                {
                    SpecialtyId = id,
                    SpecialtyName = specialty.SpecialtyName,
                    State = specialty.State
                };

                var result = await _specialtyRepository.UpdateAsync(dataSpecialty);

                if (result.OperationStatusCode == 0 && result.Data != null)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = "Registro actualizado con éxito"
                    };
                }

                if (result.OperationStatusCode == 70003)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe una Especialidad asociada al Id proporcionado"
                    };
                }

                if (result.OperationStatusCode == 5050)
                {
                    return new ServiceResponse<Specialty>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe una Especialidad con el nombre proporcionado"
                    };
                }

                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = result.Message ?? "Ocurrió un error inesperado"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Specialty>> SetStateAsync(int specialtyId, bool state)
        {
            var existingSpecialty = await _specialtyRepository.GetByIdAsync(specialtyId);

            if (existingSpecialty.OperationStatusCode == 70003 || existingSpecialty.Data == null)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "La Especialidad no existe"
                };
            }

            var repoResponse = await _specialtyRepository.SetStateAsync(specialtyId, state);

            if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = repoResponse.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = state ? "Especialidad activada correctamente" : "Especialidad desactivada correctamente"
                };
            }

            if (repoResponse.OperationStatusCode == 70003)
            {
                return new ServiceResponse<Specialty>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "La Especialidad no existe"
                };
            }

            return new ServiceResponse<Specialty>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = repoResponse.Message ?? "No se pudo actualizar el estado de la Especialidad"
            };
        }
    }
}