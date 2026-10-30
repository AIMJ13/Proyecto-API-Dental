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
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;

        public RoleService(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<ServiceResponse<IEnumerable<Role>>> GetAllAsync()
        {
            var result = await _roleRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
                return new ServiceResponse<IEnumerable<Role>>
                {
                    Data = result.Data, IsSuccess = true,
                    MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                5053 => new ServiceResponse<IEnumerable<Role>>
                {
                    Data = new List<Role>(), IsSuccess = false,
                    MessageCode = MessageCodes.NoData, Message = "No se encontraron roles registrados."
                },
                _ => new ServiceResponse<IEnumerable<Role>>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<Role>> GetByIdAsync(int id)
        {
            var result = await _roleRepository.GetByIdAsync(id);

            if (result.OperationStatusCode == 0 && result.Data != null)
                return new ServiceResponse<Role>
                {
                    Data = result.Data, IsSuccess = true,
                    MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                5051 => new ServiceResponse<Role>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.NotFound, Message = "No se encontró el rol con el Id proporcionado."
                },
                _ => new ServiceResponse<Role>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<Role>> GetByNameAsync(string name)
        {
            var result = await _roleRepository.GetByNameAsync(name);

            if (result.OperationStatusCode == 0 && result.Data != null)
                return new ServiceResponse<Role>
                {
                    Data = result.Data, IsSuccess = true,
                    MessageCode = MessageCodes.Success, Message = "Operación exitosa"
                };

            return result.OperationStatusCode switch
            {
                5051 => new ServiceResponse<Role>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.NotFound, Message = "No se encontró el rol con ese nombre."
                },
                _ => new ServiceResponse<Role>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                }
            };
        }

        public async Task<ServiceResponse<Role>> CreateAsync(CreateRoleDto dto)
        {
            try
            {
                var role = new Role { RoleName = dto.RoleName };
                var result = await _roleRepository.CreateAsync(role);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Role>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Rol creado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    5050 => new ServiceResponse<Role>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.Conflict, Message = "Ya existe un rol con ese nombre."
                    },
                    _ => new ServiceResponse<Role>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado al crear el rol."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Role>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Role>> UpdateAsync(int id, UpdateRoleDto dto)
        {
            try
            {
                var role = new Role { Id = id, RoleName = dto.RoleName };
                var result = await _roleRepository.UpdateAsync(role);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Role>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success, Message = "Rol actualizado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    5051 => new ServiceResponse<Role>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el rol con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Role>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado al actualizar el rol."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Role>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }

        public async Task<ServiceResponse<Role>> SetStateAsync(int id, bool state)
        {
            try
            {
                var result = await _roleRepository.SetStateAsync(id, state);

                if (result.OperationStatusCode == 0 && result.Data != null)
                    return new ServiceResponse<Role>
                    {
                        Data = result.Data, IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = state ? "Rol activado correctamente." : "Rol desactivado correctamente."
                    };

                return result.OperationStatusCode switch
                {
                    5051 => new ServiceResponse<Role>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.NotFound, Message = "No se encontró el rol con el Id proporcionado."
                    },
                    _ => new ServiceResponse<Role>
                    {
                        Data = null, IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase, Message = result.Message ?? "Error inesperado."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<Role>
                {
                    Data = null, IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase, Message = ex.Message
                };
            }
        }
    }
}
