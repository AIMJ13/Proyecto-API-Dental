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
    public interface IPatientService
    {
        Task<ServiceResponse<IEnumerable<Patient>>> GetAllAsync();
        Task<ServiceResponse<PagedResponse<Patient>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<ServiceResponse<IEnumerable<Patient>>> GetByPhoneAsync(string phone);
        Task<ServiceResponse<Patient>> GetByNameAsync(string name);
        Task<ServiceResponse<Patient>> GetByIdAsync(int id);
        Task<ServiceResponse<Patient>> CreateAsync(CreatePatientDto patient);
        Task<ServiceResponse<Patient>> UpdateAsync(int id, UpdatePatientDto patient);
        Task<ServiceResponse<Patient>> SetStateAsync(int id, bool state);
    }
}
