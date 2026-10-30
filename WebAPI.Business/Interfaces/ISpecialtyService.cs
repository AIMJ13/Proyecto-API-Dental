using DentalHouseWebAPI.DTOs;
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
    public interface ISpecialtyService
    {
        Task<ServiceResponse<IEnumerable<Specialty>>> GetAllAsync();
        Task<ServiceResponse<PagedResponse<Specialty>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, bool? isActive = null);
        Task<ServiceResponse<Specialty>> GetByIdAsync(int id);
        Task<ServiceResponse<Specialty>> CreateAsync(CreateSpecialtyDto newSpecialty);
        Task<ServiceResponse<Specialty>> UpdateAsync(int id,UpdateSpecialtyDto specialty);
        Task<ServiceResponse<Specialty>> GetByNameAsync(string name);
        Task<ServiceResponse<Specialty>> SetStateAsync(int SpecialtyId, bool state);
    }
}
