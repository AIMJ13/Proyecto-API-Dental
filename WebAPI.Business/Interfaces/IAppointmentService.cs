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
    public interface IAppointmentService
    {
        Task<ServiceResponse<IEnumerable<Appointment>>> GetAllAsync();
        Task<ServiceResponse<PagedResponse<Appointment>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? doctorId = null,
            int? patientId = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null);
        Task<ServiceResponse<Appointment>> GetByIdAsync(int id);
        Task<ServiceResponse<Appointment>> CreateAsync(CreateAppointmentDto dto);
        Task<ServiceResponse<Appointment>> UpdateAsync(int id, UpdateAppointmentDto dto);
        Task<ServiceResponse<bool>> DeleteAsync(int id);
        Task<ServiceResponse<Appointment>> UpdateStatusAsync(int appointmentId, string status);
    }
}