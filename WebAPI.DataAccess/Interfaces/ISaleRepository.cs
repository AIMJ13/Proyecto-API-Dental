using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface ISaleRepository
    {
        Task<RepositoryResponse<SaleTransaction>> InsertAsync(Sale master, IEnumerable<SaleDetail> details);
        Task<RepositoryResponse<SaleTransaction>> GetByIdAsync(int id);
        Task<RepositoryResponse<IEnumerable<Sale>>> GetAllAsync();

        Task<RepositoryResponse<PagedResponse<Sale>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? patientId = null,
            int? doctorId = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
    }
}
