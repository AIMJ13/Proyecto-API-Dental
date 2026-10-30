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
    public interface IDoctorService
    {
        Task<ServiceResponse<IEnumerable<Doctor>>> GetAllAsync();
        Task<ServiceResponse<(IEnumerable<Doctor> Doctors, int TotalRecords)>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            int? specialtyId = null,
            bool? isActive = null,
            string? searchTerm = null);
        Task<ServiceResponse<Doctor>> GetByIdAsync(int id);
        Task<ServiceResponse<Doctor>> CreateAsync(CreateDoctorDto newDoctor);
        Task<ServiceResponse<Doctor>> UpdateAsync(int id, UpdateDoctorDto doctor);
        Task<ServiceResponse<Doctor>> GetByNameAsync(string name);
        Task<ServiceResponse<Doctor>> SetStateAsync(int id, bool state);

    }
}
