using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IPatientRepository
    {
        Task<RepositoryResponse<IEnumerable<Patient>>> GetAllAsync();
        Task<RepositoryResponse<PagedResponse<Patient>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<RepositoryResponse<IEnumerable<Patient>>> GetByPhoneAsync(string phone);
        Task<RepositoryResponse<Patient?>> GetByIdAsync(int id);
        Task<RepositoryResponse<Patient?>> GetByNameAsync(string name);

        Task<RepositoryResponse<Patient?>> CreateAsync(Patient patient);

        Task<RepositoryResponse<Patient?>> UpdateAsync(Patient patient);

        Task<RepositoryResponse<Patient?>> SetStateAsync(int id, bool state);
    }
}
