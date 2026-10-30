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
    public interface ISaleService
    {
        Task<ServiceResponse<SaleResponseDto>> InsertAsync(CreateSaleDto dto);
        Task<ServiceResponse<SaleResponseDto>> GetByIdAsync(int id);
        Task<ServiceResponse<IEnumerable<SaleResponseDto>>> GetAllAsync();
        Task<ServiceResponse<PagedResponse<Sale>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? patientId = null,
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
