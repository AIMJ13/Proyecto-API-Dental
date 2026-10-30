using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IUserRepository
    {
        Task<RepositoryResponse<IEnumerable<User>>> GetAllAsync();

        Task<RepositoryResponse<PagedResponse<User>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null);

        Task<RepositoryResponse<User>> GetByIdAsync(int id);

        Task<RepositoryResponse<User>> GetByNameAsync(string name);

        Task<RepositoryResponse<User>> AddAsync(User user);

        Task<RepositoryResponse<User>> UpdateAsync(int id, User user);

        Task<RepositoryResponse<User>> SetStateAsync(int userId, bool state);
        Task<RepositoryResponse<UserRole>> AssignUserRoleAsync(int userId, int roleId);
        Task<RepositoryResponse<bool>> UpdatePasswordAsync(int userId, string passwordHash);
        Task<RepositoryResponse<bool>> SyncUserRolesAsync(int userId, IEnumerable<int> roleIds);
    }
}
