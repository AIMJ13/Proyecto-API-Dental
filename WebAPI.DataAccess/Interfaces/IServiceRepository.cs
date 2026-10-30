using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IServiceRepository
    {
        Task<RepositoryResponse<IEnumerable<Service>>> GetAllAsync();
        Task<RepositoryResponse<PagedResponse<Service>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, decimal? minCost = null, decimal? maxCost = null, bool? isActive = null);
        Task<RepositoryResponse<Service?>> GetByNameAsync(string name);
        Task<RepositoryResponse<Service?>> GetByIdAsync(int id);
        Task<RepositoryResponse<Service?>> AddAsync(Service service);
        Task<RepositoryResponse<Service?>> UpdateAsync(Service service);
        Task<RepositoryResponse<Service>> SetStateAsync(int id, bool state);

    }
}
