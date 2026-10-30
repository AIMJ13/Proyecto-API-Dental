using BCrypt.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.Business.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _authRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IConfiguration  _configuration;

        public AuthService(
            IAuthRepository authRepository,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IConfiguration configuration)
        {
            _authRepository = authRepository;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _configuration  = configuration;
        }

        private string GenerateTokenJWT(User user, IEnumerable<string> roles)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey no configurado");
            var issuer    = _configuration["JwtSettings:Issuer"]
                ?? throw new InvalidOperationException("JWT Issuer no configurado");
            var audience  = _configuration["JwtSettings:Audience"]
                ?? throw new InvalidOperationException("JWT Audience no configurado");

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name,  user.UserName),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString())
            };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationHours = _configuration.GetValue<int>("JwtSettings:ExpirationHours", 3);

            var token = new JwtSecurityToken(
                issuer:             issuer,
                audience:           audience,
                claims:             claims,
                notBefore:          DateTime.UtcNow,
                expires:            DateTime.UtcNow.AddHours(expirationHours),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<ServiceResponse<User>> RegisterAsync(RegisterUserDto newUser)
        {
            try
            {
                var existingUser = await _authRepository.GetByUserNameAsync(newUser.UserName);
                if (existingUser.OperationStatusCode == 0 && existingUser.Data != null && existingUser.Data.Id > 0)
                {
                    return new ServiceResponse<User>
                    {
                        Data        = null,
                        IsSuccess   = false,
                        MessageCode = MessageCodes.Conflict,
                        Message     = "Ya existe un usuario con ese nombre de usuario."
                    };
                }

                var userEntity = new User
                {
                    UserName     = newUser.UserName,
                    Email        = newUser.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(newUser.Password),
                    IsActive     = true
                };

                var repoResponse = await _authRepository.RegisterAsync(userEntity);

                if (repoResponse.OperationStatusCode == 0 && repoResponse.Data != null)
                {
                    var rolesFromDb = await _roleRepository.GetAllAsync();
                    var availableRoles = rolesFromDb.Data?.ToList() ?? new List<Role>();

                    var rolesToAssign = (newUser.Roles != null && newUser.Roles.Any())
                        ? newUser.Roles
                        : new List<string> { "Recepcionista" };

                    foreach (var roleItem in rolesToAssign)
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
                            await _userRepository.AssignUserRoleAsync(repoResponse.Data.Id, roleId);
                            if (!repoResponse.Data.Roles.Contains(roleName))
                            {
                                repoResponse.Data.Roles.Add(roleName);
                            }
                        }
                    }

                    return new ServiceResponse<User>
                    {
                        Data        = repoResponse.Data,
                        IsSuccess   = true,
                        MessageCode = MessageCodes.Success,
                        Message     = "Usuario registrado correctamente."
                    };
                }

                return repoResponse.OperationStatusCode switch
                {
                    5050 => new ServiceResponse<User>
                    {
                        Data        = null,
                        IsSuccess   = false,
                        MessageCode = MessageCodes.Conflict,
                        Message     = "El nombre de usuario o correo ya existe."
                    },
                    _ => new ServiceResponse<User>
                    {
                        Data        = null,
                        IsSuccess   = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message     = repoResponse.Message ?? "Error inesperado al registrar el usuario."
                    }
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<User>
                {
                    Data        = null,
                    IsSuccess   = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message     = $"Error inesperado: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<User>> GetByUserNameAsync(string username)
        {
            var result = await _authRepository.GetByUserNameAsync(username);

            if (result.OperationStatusCode == 0 && result.Data != null && result.Data.Id > 0)
            {
                return new ServiceResponse<User>
                {
                    Data        = result.Data,
                    IsSuccess   = true,
                    MessageCode = MessageCodes.Success,
                    Message     = "Operación exitosa"
                };
            }

            return result.OperationStatusCode switch
            {
                5051 => new ServiceResponse<User>
                {
                    Data        = null,
                    IsSuccess   = false,
                    MessageCode = MessageCodes.NotFound,
                    Message     = "No se encontró el usuario con ese nombre de usuario."
                },
                _ => new ServiceResponse<User>
                {
                    Data        = null,
                    IsSuccess   = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message     = result.Message ?? "Error en la base de datos."
                }
            };
        }

        public async Task<ServiceResponse<User>> GetByEmailAsync(string email)
        {
            var result = await _authRepository.GetByEmailAsync(email);

            if (result.OperationStatusCode == 0 && result.Data != null && result.Data.Id > 0)
            {
                return new ServiceResponse<User>
                {
                    Data        = result.Data,
                    IsSuccess   = true,
                    MessageCode = MessageCodes.Success,
                    Message     = "Operación exitosa"
                };
            }

            return result.OperationStatusCode switch
            {
                5052 => new ServiceResponse<User>
                {
                    Data        = null,
                    IsSuccess   = false,
                    MessageCode = MessageCodes.NotFound,
                    Message     = "No se encontró usuario con el correo proporcionado."
                },
                _ => new ServiceResponse<User>
                {
                    Data        = null,
                    IsSuccess   = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message     = result.Message ?? "Error en la base de datos."
                }
            };
        }

        public async Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var userResponse = await _authRepository.GetByUserNameAsync(loginRequest.UserName);

                if (userResponse.OperationStatusCode != 0 || userResponse.Data == null || userResponse.Data.Id == 0)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data        = null,
                        IsSuccess   = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message     = "No existe un usuario registrado con ese nombre de usuario."
                    };
                }

                var isValidPassword = BCrypt.Net.BCrypt.Verify(loginRequest.Password, userResponse.Data.PasswordHash);
                if (!isValidPassword)
                {
                    return new ServiceResponse<LoginResponseDto>
                    {
                        Data        = null,
                        IsSuccess   = false,
                        MessageCode = MessageCodes.Unauthorized,
                        Message     = "La contraseña no coincide con la registrada."
                    };
                }

                var rolesResponse = await _authRepository.GetRolesByUserIdAsync(userResponse.Data.Id);
                var roles         = rolesResponse.Data ?? new List<string>();

                var token = GenerateTokenJWT(userResponse.Data, roles);

                var loginResponse = new LoginResponseDto
                {
                    Id       = userResponse.Data.Id,
                    UserName = userResponse.Data.UserName,
                    Email    = userResponse.Data.Email ?? "",
                    Token    = token,
                    Roles    = roles
                };

                return new ServiceResponse<LoginResponseDto>
                {
                    Data        = loginResponse,
                    IsSuccess   = true,
                    MessageCode = MessageCodes.Success,
                    Message     = "Login exitoso."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<LoginResponseDto>
                {
                    Data        = null,
                    IsSuccess   = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message     = $"Error inesperado: {ex.Message}"
                };
            }
        }

        public async Task<ServiceResponse<UserDto>> GetProfileAsync(int userId)
        {
            try
            {
                var userResponse = await _authRepository.GetByIdAsync(userId);

                if (userResponse.OperationStatusCode != 0 || userResponse.Data == null || userResponse.Data.Id == 0)
                {
                    return new ServiceResponse<UserDto>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "Usuario no encontrado."
                    };
                }

                var rolesResponse = await _authRepository.GetRolesByUserIdAsync(userId);
                var rolesList = rolesResponse.Data != null ? rolesResponse.Data.ToList() : new List<string>();

                var profileDto = new UserDto
                {
                    Id = userResponse.Data.Id,
                    UserName = userResponse.Data.UserName,
                    Email = userResponse.Data.Email,
                    State = userResponse.Data.IsActive,
                    Roles = rolesList
                };

                return new ServiceResponse<UserDto>
                {
                    Data = profileDto,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Perfil obtenido correctamente."
                };
            }
            catch (Exception ex)
            {
                return new ServiceResponse<UserDto>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = $"Error al obtener el perfil: {ex.Message}"
                };
            }
        }
    }
}
