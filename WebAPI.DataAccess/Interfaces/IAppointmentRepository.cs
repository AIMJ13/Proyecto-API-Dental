using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IAppointmentRepository
    {
        Task<RepositoryResponse<IEnumerable<Appointment>>> GetAllAsync();
        Task<RepositoryResponse<PagedResponse<Appointment>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? doctorId = null,
            int? patientId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<RepositoryResponse<Appointment?>> GetByIdAsync(int id);
        Task<RepositoryResponse<Appointment?>> CreateAsync(Appointment appointment);
        Task<RepositoryResponse<Appointment?>> UpdateAsync(Appointment appointment);
        Task<RepositoryResponse<bool>> DeleteAsync(int id);
        Task<RepositoryResponse<Appointment>> UpdateStatusAsync(int appointmentId, string status);
    }
}
