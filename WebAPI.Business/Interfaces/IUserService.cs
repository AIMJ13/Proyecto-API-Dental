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
    public interface IUserService
    {
        Task<ServiceResponse<IEnumerable<User>>> GetAllAsync();
        Task<ServiceResponse<PagedResponse<User>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null);
        Task<ServiceResponse<User>> GetByIdAsync(int ids);
        Task<ServiceResponse<User>> GetByNameAsync(string name);
        Task<ServiceResponse<User>> CreateAsync(CreateUserDto newUser);
        Task<ServiceResponse<User>> UpdateAsync(int id, UpdateUserDto user);

        Task<ServiceResponse<User>> SetStateAsync(int userId, bool state);
        Task<ServiceResponse<UserRole>> AssignUserRoleAsync(int userId, int roleId);
        Task<ServiceResponse<bool>> ResetPasswordAsync(int userId, string newPassword);
        Task<ServiceResponse<bool>> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<ServiceResponse<bool>> SyncUserRolesAsync(int userId, string roles);
    }
}
