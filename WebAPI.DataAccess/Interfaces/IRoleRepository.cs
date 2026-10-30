using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IRoleRepository
    {
        Task<RepositoryResponse<IEnumerable<Role>>> GetAllAsync();
        Task<RepositoryResponse<Role>> GetByIdAsync(int id);
        Task<RepositoryResponse<Role>> GetByNameAsync(string name);

        Task<RepositoryResponse<Role>> CreateAsync(Role role);

        Task<RepositoryResponse<Role>> UpdateAsync(Role role);

        Task<RepositoryResponse<Role>> SetStateAsync(int roleId, bool state);
    }
}
