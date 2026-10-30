using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.Business.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResponse<User>> RegisterAsync(RegisterUserDto newUser);
        Task<ServiceResponse<User>> GetByUserNameAsync(string username);
        Task<ServiceResponse<User>> GetByEmailAsync(string email);
        Task<ServiceResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
        Task<ServiceResponse<UserDto>> GetProfileAsync(int userId);
    }
}
