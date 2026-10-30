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
    public interface IServiceService
    {
        Task<ServiceResponse<IEnumerable<Service>>> GetAllAsync();
        Task<ServiceResponse<PagedResponse<Service>>> GetPagedAsync(int pageNumber, int pageSize, string? searchTerm = null, decimal? minCost = null, decimal? maxCost = null, bool? isActive = null);
        Task<ServiceResponse<Service>> GetByIdAsync(int id);
        Task<ServiceResponse<Service>> CreateAsync(CreateServiceDto newService);
        Task<ServiceResponse<Service>> UpdateAsync(UpdateServiceDto service);
        Task<ServiceResponse<Service>> SetStateAsync(int serviceId, bool state);
        Task<ServiceResponse<Service>> GetByNameAsync(string name);
    }
}
