using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IAuthRepository
    {
        Task<RepositoryResponse<User>> RegisterAsync(User user);
        Task<RepositoryResponse<User>> GetByUserNameAsync(string userName);
        Task<RepositoryResponse<User>> GetByIdAsync(int id);
        Task<RepositoryResponse<User>> GetByEmailAsync(string email);
        Task<RepositoryResponse<IEnumerable<string>>> GetRolesByUserIdAsync(int userId);
     
    }
}
