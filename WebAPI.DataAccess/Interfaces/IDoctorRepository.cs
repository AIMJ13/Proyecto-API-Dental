using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IDoctorRepository
    {
        Task<RepositoryResponse<IEnumerable<Doctor>>> GetAllAsync();

        Task<RepositoryResponse<(IEnumerable<Doctor> Doctors, int TotalRecords)>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? specialtyId = null,
            bool? isActive = null,
            string? searchTerm = null);

        Task<RepositoryResponse<Doctor?>> GetByIdAsync(int id);
        Task<RepositoryResponse<Doctor?>> GetByNameAsync(string name);
        Task<RepositoryResponse<Doctor?>> CreateAsync(Doctor doctor);
        Task<RepositoryResponse<Doctor?>> UpdateAsync(Doctor doctor);
        Task<RepositoryResponse<Doctor?>> SetStateAsync(int id, bool state);
    }
}