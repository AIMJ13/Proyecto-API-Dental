using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.Business.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public UserService(IUserRepository userRepository, IRoleRepository roleRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
        }

        
        public async Task<ServiceResponse<IEnumerable<User>>> GetAllAsync()
        {
            var result = await _userRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<User>>()
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }



            switch (result.OperationStatusCode)
            {
                case 5028:
                    return new ServiceResponse<IEnumerable<User>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontaron registros"
                    };

                default:
                    return new ServiceResponse<IEnumerable<User>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrio un error inesperado"
                    };

            }

        }

        public async Task<ServiceResponse<PagedResponse<User>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null)
        {
            var result = await _userRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, isActive);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<PagedResponse<User>>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            switch (result.OperationStatusCode)
            {
                case 5053:
                    return new ServiceResponse<PagedResponse<User>>
                    {
                        Data = result.Data ?? new PagedResponse<User> { PageNumber = pageNumber, PageSize = pageSize },
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontraron registros"
                    };

                default:
                    return new ServiceResponse<PagedResponse<User>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrio un error inesperado"
                    };
            }
        }

        public async Task<ServiceResponse<User>> GetByIdAsync(int id)
        {
            var repoResponse = await _userRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<User>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"

                    };
                }

                switch (repoResponse.OperationStatusCode)
                {
                    case 5051:
                        return new ServiceResponse<User>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"

                        };


                    default:
                        return new ServiceResponse<User>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repoResponse.Message ?? "Ocurrio un error inesperado"

                        };

                }
            }
            catch (Exception)
            {
                return new ServiceResponse<User>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"

                };


            }
        }

        public async Task<ServiceResponse<User>> GetByNameAsync(string name)
        {
            var result = await _userRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<User>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            var messageCode = new MessageCodes();
            var message = string.Empty;

            switch (result.OperationStatusCode)
            {
                case 5051:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontro el usuario con ese Nombre proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el cliente.";
                    break;
            }

            return new ServiceResponse<User>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<User>> CreateAsync(CreateUserDto newUser)
        {
            try
            {
                var existingUser = await _userRepository.GetByNameAsync(newUser.UserName);

                if (existingUser.Data != null && existingUser.Data.Id != 0)
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"
                    };
                }

                var user = new User()
                {
                    UserName = newUser.UserName,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.PasswordHash),
                    Email = newUser.Email,
                    IsActive = true
                };

                var result = await _userRepository.AddAsync(user);

                if (result.OperationStatusCode != 0 || result.Data == null || result.Data.Id == 0)
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = result.OperationStatusCode == 5050 ? MessageCodes.Conflict : MessageCodes.ErrorDataBase,
                        Message = result.Message ?? "Ocurrio un error al registrar el usuario"
                    };
                }

                if (!string.IsNullOrWhiteSpace(newUser.Roles))
                {
                    var rolesFromDb = await _roleRepository.GetAllAsync();
                    var availableRoles = rolesFromDb.Data?.ToList() ?? new List<Role>();

                    var requestedRoles = newUser.Roles.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

                    foreach (var roleItem in requestedRoles)
                    {
                        int roleId = 0;
                        string roleName = roleItem;

                        if (int.TryParse(roleItem, out int parsedId))
                        {
                            var matchedById = availableRoles.FirstOrDefault(r => r.Id == parsedId);
                            if (matchedById != null)
                            {
                                roleId = matchedById.Id;
                                roleName = matchedById.RoleName;
                            }
                            else
                            {
                                roleId = parsedId;
                            }
                        }
                        else
                        {
                            var matched = availableRoles.FirstOrDefault(r => string.Equals(r.RoleName, roleItem, StringComparison.OrdinalIgnoreCase));
                            if (matched != null)
                            {
                                roleId = matched.Id;
                                roleName = matched.RoleName;
                            }
                            else
                            {
                                var lower = roleItem.ToLowerInvariant();
                                if (lower.Contains("admin"))
                                {
                                    roleId = 1;
                                    roleName = "Administrador";
                                }
                                else if (lower.Contains("recep"))
                                {
                                    roleId = 2;
                                    roleName = "Recepcionista";
                                }
                                else if (lower.Contains("doc"))
                                {
                                    roleId = 3;
                                    roleName = "Doctor";
                                }
                            }
                        }

                        if (roleId > 0)
                        {
                            await _userRepository.AssignUserRoleAsync(result.Data.Id, roleId);
                            if (!result.Data.Roles.Contains(roleName))
                            {
                                result.Data.Roles.Add(roleName);
                            }
                        }
                    }
                }

                return new ServiceResponse<User>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<User>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Ocurrio un error inesperado: {ex.Message}",
                };
            }
        }


        public async Task<ServiceResponse<User>> UpdateAsync(int id, UpdateUserDto user)
        {
            try
            {
                var existingIdUser = await _userRepository.GetByIdAsync(id);
                if (existingIdUser.Data == null || existingIdUser.Data.Id == 0)
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un usuario asociado al Id proporcionado"
                    };
                }

                var existingNameUser = await _userRepository.GetByNameAsync(user.UserName);
                if (existingNameUser.Data != null && existingNameUser.Data.Id != id)
                {
                    return new ServiceResponse<User>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Ya existe un usuario con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                if (!string.IsNullOrWhiteSpace(user.PasswordHash))
                {
                    string newHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
                    await _userRepository.UpdatePasswordAsync(id, newHash);
                }

                if (user.State.HasValue)
                {
                    await _userRepository.SetStateAsync(id, user.State.Value);
                }

                if (user.Roles != null)
                {
                    await SyncUserRolesAsync(id, user.Roles);
                }

                var dataUser = new User()
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    IsActive = user.State ?? existingIdUser.Data.IsActive,
                };

                var result = await _userRepository.UpdateAsync(id, dataUser);
                var updatedUser = await _userRepository.GetByIdAsync(id);

                return new ServiceResponse<User>
                {
                    Data = updatedUser.Data ?? result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<User>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }




        public async Task<ServiceResponse<User>> SetStateAsync(int userId, bool state)
        {
            var response = new ServiceResponse<User>();

            var existingUser = await _userRepository.GetByIdAsync(userId);
            if (existingUser == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "El usuario no existe";
                return response;
            }


            var repoResponse = await _userRepository.SetStateAsync(userId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del usuario";
                return response;
            }


            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Usuario activado" : "Usuario desactivado";

            return response;
        }



        public async Task<ServiceResponse<UserRole>> AssignUserRoleAsync(int userId, int roleId)
        {
            var result = await _userRepository.AssignUserRoleAsync(userId, roleId);

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<UserRole>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Rol asignado correctamente."
                };
            }

            return result.OperationStatusCode switch
            {
                5051 => new ServiceResponse<UserRole>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "Usuario no encontrado."
                },
                5054 => new ServiceResponse<UserRole>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "Rol no encontrado."
                },
                5050 => new ServiceResponse<UserRole>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.Conflict,
                    Message = "El rol ya está asignado a este usuario."
                },
                _ => new ServiceResponse<UserRole>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Error inesperado al asignar el rol."
                }
            };
        }

        public async Task<ServiceResponse<bool>> ResetPasswordAsync(int userId, string newPassword)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser.Data == null || existingUser.Data.Id == 0)
                {
                    return new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un usuario asociado al Id proporcionado"
                    };
                }

                string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var repoResult = await _userRepository.UpdatePasswordAsync(userId, hash);

                if (!repoResult.Data)
                {
                    return new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = repoResult.Message ?? "Error al actualizar la contraseña"
                    };
                }

                return new ServiceResponse<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Contraseña restablecida exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Ocurrió un error inesperado: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser.Data == null || existingUser.Data.Id == 0)
                {
                    return new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un usuario asociado al Id proporcionado"
                    };
                }

                if (string.IsNullOrEmpty(existingUser.Data.PasswordHash) ||
                    !BCrypt.Net.BCrypt.Verify(currentPassword, existingUser.Data.PasswordHash))
                {
                    return new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorValidation,
                        Message = "La contraseña actual es incorrecta"
                    };
                }

                string hash = BCrypt.Net.BCrypt.HashPassword(newPassword);
                var repoResult = await _userRepository.UpdatePasswordAsync(userId, hash);

                if (!repoResult.Data)
                {
                    return new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = repoResult.Message ?? "Error al actualizar la contraseña"
                    };
                }

                return new ServiceResponse<bool>
                {
                    Data = true,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Contraseña cambiada exitosamente"
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Ocurrió un error inesperado: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<bool>> SyncUserRolesAsync(int userId, string roles)
        {
            try
            {
                var existingUser = await _userRepository.GetByIdAsync(userId);
                if (existingUser.Data == null || existingUser.Data.Id == 0)
                {
                    return new ServiceResponse<bool>
                    {
                        Data = false,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "Usuario no encontrado."
                    };
                }

                var rolesFromDb = await _roleRepository.GetAllAsync();
                var availableRoles = rolesFromDb.Data?.ToList() ?? new List<Role>();
                var requestedRoles = roles.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var roleIds = new List<int>();

                foreach (var roleItem in requestedRoles)
                {
                    if (int.TryParse(roleItem, out int parsedId))
                    {
                        var matchedById = availableRoles.FirstOrDefault(r => r.Id == parsedId);
                        if (matchedById != null) roleIds.Add(matchedById.Id);
                        else roleIds.Add(parsedId);
                    }
                    else
                    {
                        var matched = availableRoles.FirstOrDefault(r => string.Equals(r.RoleName, roleItem, StringComparison.OrdinalIgnoreCase));
                        if (matched != null)
                        {
                            roleIds.Add(matched.Id);
                        }
                        else
                        {
                            var lower = roleItem.ToLowerInvariant();
                            if (lower.Contains("admin")) roleIds.Add(1);
                            else if (lower.Contains("recep")) roleIds.Add(2);
                            else if (lower.Contains("doc")) roleIds.Add(3);
                        }
                    }
                }

                var repoResult = await _userRepository.SyncUserRolesAsync(userId, roleIds.Distinct());
                return new ServiceResponse<bool>
                {
                    Data = repoResult.Data,
                    IsSuccess = repoResult.Data,
                    MessageCode = repoResult.Data ? MessageCodes.Success : MessageCodes.ErrorDataBase,
                    Message = repoResult.Data ? "Roles sincronizados correctamente." : "Error al sincronizar roles."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<bool>
                {
                    Data = false,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Error inesperado al sincronizar roles: {ex.Message}"
                };
            }
        }




    }
}
