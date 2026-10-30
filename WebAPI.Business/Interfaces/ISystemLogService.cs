using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Core.Common;

namespace WebAPI.Business.Interfaces
{
    public interface ISystemLogService
    {
        Task<ServiceResponse<int>> LogAsync(CreateSystemLogDto dto);

        Task<ServiceResponse<PagedResponse<SystemLogDto>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? module = null,
            string? action = null,
            string? userName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            bool? isSuccess = null);

        Task<ServiceResponse<SystemLogDto?>> GetByIdAsync(int id);

        Task<ServiceResponse<IEnumerable<string>>> GetModulesAsync();

        Task<ServiceResponse<IEnumerable<string>>> GetActionsAsync();
    }
}
