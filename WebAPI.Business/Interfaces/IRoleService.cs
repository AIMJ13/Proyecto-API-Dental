using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.Business.Interfaces
{
    public interface IRoleService
    {
        Task<ServiceResponse<IEnumerable<Role>>> GetAllAsync();
        Task<ServiceResponse<Role>> GetByIdAsync(int ids);
        Task<ServiceResponse<Role>> GetByNameAsync(string name);
        Task<ServiceResponse<Role>> CreateAsync(CreateRoleDto newRole);
        Task<ServiceResponse<Role>> UpdateAsync(int id, UpdateRoleDto role);
        Task<ServiceResponse<Role>> SetStateAsync(int roleId, bool state);
    }
}
