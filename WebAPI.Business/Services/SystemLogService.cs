using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;

namespace WebAPI.Business.Services
{
    public class SystemLogService : ISystemLogService
    {
        private readonly ISystemLogRepository _repository;

        public SystemLogService(ISystemLogRepository repository)
        {
            _repository = repository;
        }

        private static SystemLogDto MapToDto(SystemLog log)
        {
            return new SystemLogDto
            {
                LogId = log.LogId,
                Timestamp = log.Timestamp,
                UserId = log.UserId,
                UserName = log.UserName,
                UserRole = log.UserRole,
                Action = log.Action,
                Module = log.Module,
                EntityId = log.EntityId,
                Description = log.Description,
                Details = log.Details,
                IpAddress = log.IpAddress,
                IsSuccess = log.IsSuccess
            };
        }

        public async Task<ServiceResponse<int>> LogAsync(CreateSystemLogDto dto)
        {
            var entity = new SystemLog
            {
                UserId = dto.UserId,
                UserName = string.IsNullOrWhiteSpace(dto.UserName) ? "Sistema" : dto.UserName,
                UserRole = dto.UserRole,
                Action = dto.Action,
                Module = dto.Module,
                EntityId = dto.EntityId,
                Description = dto.Description,
                Details = dto.Details,
                IpAddress = dto.IpAddress,
                IsSuccess = dto.IsSuccess
            };

            var repoResponse = await _repository.InsertAsync(entity);

            if (repoResponse.IsSuccess)
            {
                return new ServiceResponse<int>
                {
                    Data = repoResponse.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = repoResponse.Message
                };
            }

            return new ServiceResponse<int>
            {
                Data = 0,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = repoResponse.Message ?? "Error al registrar el log."
            };
        }

        public async Task<ServiceResponse<PagedResponse<SystemLogDto>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? module = null,
            string? action = null,
            string? userName = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? searchTerm = null,
            bool? isSuccess = null)
        {
            var repoResponse = await _repository.GetPagedAsync(
                pageNumber,
                pageSize,
                module,
                action,
                userName,
                startDate,
                endDate,
                searchTerm,
                isSuccess);

            if (repoResponse.IsSuccess && repoResponse.Data != null)
            {
                var dtos = repoResponse.Data.Data.Select(MapToDto).ToList();

                var pagedResponse = new PagedResponse<SystemLogDto>
                {
                    PageNumber = repoResponse.Data.PageNumber,
                    PageSize = repoResponse.Data.PageSize,
                    TotalRecords = repoResponse.Data.TotalRecords,
                    TotalPages = repoResponse.Data.TotalPages,
                    Data = dtos
                };

                return new ServiceResponse<PagedResponse<SystemLogDto>>
                {
                    Data = pagedResponse,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Solicitud exitosa."
                };
            }

            return new ServiceResponse<PagedResponse<SystemLogDto>>
            {
                Data = null!,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = repoResponse.Message ?? "Error al consultar los logs del sistema."
            };
        }

        public async Task<ServiceResponse<SystemLogDto?>> GetByIdAsync(int id)
        {
            var repoResponse = await _repository.GetByIdAsync(id);

            if (repoResponse.IsSuccess && repoResponse.Data != null)
            {
                return new ServiceResponse<SystemLogDto?>
                {
                    Data = MapToDto(repoResponse.Data),
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro encontrado."
                };
            }

            if (repoResponse.OperationStatusCode == 404)
            {
                return new ServiceResponse<SystemLogDto?>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.NotFound,
                    Message = "No se encontró el registro de log con el Id proporcionado."
                };
            }

            return new ServiceResponse<SystemLogDto?>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = repoResponse.Message ?? "Error al consultar el log."
            };
        }

        public async Task<ServiceResponse<IEnumerable<string>>> GetModulesAsync()
        {
            var repoResponse = await _repository.GetModulesAsync();

            if (repoResponse.IsSuccess && repoResponse.Data != null)
            {
                return new ServiceResponse<IEnumerable<string>>
                {
                    Data = repoResponse.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Módulos obtenidos correctamente."
                };
            }

            return new ServiceResponse<IEnumerable<string>>
            {
                Data = Enumerable.Empty<string>(),
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = repoResponse.Message ?? "Error al obtener módulos."
            };
        }

        public async Task<ServiceResponse<IEnumerable<string>>> GetActionsAsync()
        {
            var repoResponse = await _repository.GetActionsAsync();

            if (repoResponse.IsSuccess && repoResponse.Data != null)
            {
                return new ServiceResponse<IEnumerable<string>>
                {
                    Data = repoResponse.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Acciones obtenidas correctamente."
                };
            }

            return new ServiceResponse<IEnumerable<string>>
            {
                Data = Enumerable.Empty<string>(),
                IsSuccess = false,
                MessageCode = MessageCodes.ErrorDataBase,
                Message = repoResponse.Message ?? "Error al obtener acciones."
            };
        }
    }
}
