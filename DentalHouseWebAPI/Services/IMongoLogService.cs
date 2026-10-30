using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DentalHouseWebAPI.Models;
using WebAPI.Core.Common;

namespace DentalHouseWebAPI.Services
{
    public interface IMongoLogService
    {
        Task InsertLogAsync(MongoSystemLog log);

        Task<PagedResponse<MongoSystemLog>> GetPagedLogsAsync(
            int pageNumber,
            int pageSize,
            string? module = null,
            string? action = null,
            string? userName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            bool? isSuccess = null);

        Task<MongoSystemLog?> GetLogByIdAsync(string id);

        Task<List<string>> GetModulesAsync();

        Task<List<string>> GetActionsAsync();
    }
}
