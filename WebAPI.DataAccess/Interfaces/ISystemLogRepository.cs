using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface ISystemLogRepository
    {
        Task<RepositoryResponse<int>> InsertAsync(SystemLog log);

        Task<RepositoryResponse<PagedResponse<SystemLog>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? module = null,
            string? action = null,
            string? userName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            bool? isSuccess = null);

        Task<RepositoryResponse<SystemLog?>> GetByIdAsync(int id);

        Task<RepositoryResponse<IEnumerable<string>>> GetModulesAsync();

        Task<RepositoryResponse<IEnumerable<string>>> GetActionsAsync();
    }
}
