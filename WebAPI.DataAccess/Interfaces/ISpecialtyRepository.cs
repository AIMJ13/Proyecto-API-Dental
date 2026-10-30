using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;


namespace WebAPI.DataAccess.Interfaces
{
    public interface ISpecialtyRepository
    {
        Task<RepositoryResponse<IEnumerable<Specialty>>> GetAllAsync();
        Task<RepositoryResponse<PagedResponse<Specialty>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);

        Task<RepositoryResponse<Specialty?>> GetByIdAsync(int id);
        Task<RepositoryResponse<Specialty?>> GetByNameAsync(string name);
        Task<RepositoryResponse<Specialty?>> AddAsync(Specialty specialty);
        Task<RepositoryResponse<Specialty?>> UpdateAsync(Specialty specialty);
        Task<RepositoryResponse<Specialty>> SetStateAsync(int id, bool state);
    }
}
