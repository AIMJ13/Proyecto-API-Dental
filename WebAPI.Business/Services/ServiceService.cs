
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;
using WebAPI.DataAccess.Interfaces;
using WebAPI.DataAccess.Repositories;

namespace WebAPI.Business.Services
{
    public class ServiceService : IServiceService
    {
        private readonly IServiceRepository _serviceRepository;

        public ServiceService(IServiceRepository serviceRepository)
        {
            _serviceRepository = serviceRepository;
        }


        public async Task<ServiceResponse<IEnumerable<Service>>> GetAllAsync()
        {
            var result = await _serviceRepository.GetAllAsync();

            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<IEnumerable<Service>>()
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }



            switch (result.OperationStatusCode)
            {
                case 5024:
                    return new ServiceResponse<IEnumerable<Service>>
                    {
                        Data = result.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.NoData,
                        Message = "No se encontaron registros"
                    };

                default:
                    return new ServiceResponse<IEnumerable<Service>>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.ErrorDataBase,
                        Message = "Ocurrio un error inesperado"
                    };

            }


        }

        public async Task<ServiceResponse<PagedResponse<Service>>> GetPagedAsync(
            int pageNumber,
            int pageSize,
            string? searchTerm = null,
            decimal? minCost = null,
            decimal? maxCost = null,
            bool? isActive = null)
        {
            var result = await _serviceRepository.GetPagedAsync(pageNumber, pageSize, searchTerm, minCost, maxCost, isActive);

            return new ServiceResponse<PagedResponse<Service>>
            {
                Data = result.Data,
                IsSuccess = true,
                MessageCode = MessageCodes.Success,
                Message = "Operación exitosa"
            };
        }


        public async Task<ServiceResponse<Service>> GetByIdAsync(int id)
        {
            var repoResponse = await _serviceRepository.GetByIdAsync(id);

            try
            {
                if (repoResponse.OperationStatusCode == 0)
                {
                    return new ServiceResponse<Service>
                    {
                        Data = repoResponse.Data,
                        IsSuccess = true,
                        MessageCode = MessageCodes.Success,
                        Message = repoResponse.Message ?? "Operacion exitosa"

                    };
                }

                switch (repoResponse.OperationStatusCode)
                {
                    case 5032:
                        return new ServiceResponse<Service>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.NotFound,
                            Message = repoResponse.Message ?? "No se encontro registro asociado  al Id proporcionado"

                        };


                    default:
                        return new ServiceResponse<Service>
                        {
                            Data = null,
                            IsSuccess = false,
                            MessageCode = MessageCodes.ErrorDataBase,
                            Message = repoResponse.Message ?? "Ocurrio un error inesperado"

                        };

                }
            }
            catch (Exception)
            {
                return new ServiceResponse<Service>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = repoResponse.Message ?? "Ocurrio un error inesperado"

                };


            }
        }



        public async Task<ServiceResponse<Service>> GetByNameAsync(string name)
        {
            var result = await _serviceRepository.GetByNameAsync(name);
            if (result.OperationStatusCode == 0)
            {
                return new ServiceResponse<Service>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Operacion exitosa"
                };
            }

            var messageCode = new MessageCodes();
            var message = string.Empty;

            switch (result.OperationStatusCode)
            {
                case 70034:
                    messageCode = MessageCodes.NotFound;
                    message = "No se encontro el Servicio con ese Name proporcionado";
                    break;

                default:
                    messageCode = MessageCodes.ErrorDataBase;
                    message = "Error en la base de datos al obtener el Servicio";
                    break;
            }


            return new ServiceResponse<Service>
            {
                Data = null,
                IsSuccess = false,
                MessageCode = messageCode,
                Message = message
            };
        }


        public async Task<ServiceResponse<Service>> CreateAsync(CreateServiceDto newService)
        {
            try
            {

                var existingServiceId = await _serviceRepository.GetByNameAsync(newService.ServiceName);

                if (existingServiceId.Data != null && existingServiceId.Data.ServiceId != 0)
                {
                    return new ServiceResponse<Service>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.Conflict,
                        Message = "Existe un registro con el nombre proporcionado"

                    };

                }

                var service = new Service()
                {
                    ServiceId= newService.ServiceId,
                    ServiceName=newService.ServiceName,
                    Cost = newService.Cost,
                    State = newService.State,

                };

                var result = await _serviceRepository.AddAsync(service);

                return new ServiceResponse<Service>
                {

                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro creado con exito",
                };


            }
            catch (Exception)
            {
                return new ServiceResponse<Service>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }

        }




        public async Task<ServiceResponse<Service>> UpdateAsync(UpdateServiceDto service)
        {
            try
            {

                var existingServiceId = await _serviceRepository.GetByIdAsync(service.ServiceId);
                if (existingServiceId.Data!.ServiceId == 0 && existingServiceId.Data.ServiceName.IsNullOrEmpty())
                {
                    return new ServiceResponse<Service>
                    {
                        Data = null,
                        IsSuccess = false,
                        MessageCode = MessageCodes.NotFound,
                        Message = "No existe un Servicio asociado al Id proporcionado"

                    };
                }


                var existingService = await _serviceRepository.GetByNameAsync(service.ServiceName);
                if (existingService.Data != null && existingService.Data.ServiceId != 0 && existingService.Data.ServiceId != service.ServiceId)
                {
                    return new ServiceResponse<Service>
                    {
                        Data = null,
                        IsSuccess = false,



                        MessageCode = MessageCodes.Conflict,
                        Message = "ya existe un Servicio con el nombre proporcionado. No se debe duplicar el nombre"
                    };
                }

                var dataService = new Service()
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    Cost = service.Cost,
                    State = service.State,
                };

                var result = await _serviceRepository.UpdateAsync(dataService);

                return new ServiceResponse<Service>
                {
                    Data = result.Data,
                    IsSuccess = true,
                    MessageCode = MessageCodes.Success,
                    Message = "Registro actualizado con exito",
                };
            }
            catch (Exception)
            {
                return new ServiceResponse<Service>
                {
                    Data = null,
                    IsSuccess = false,
                    MessageCode = MessageCodes.ErrorDataBase,
                    Message = "Ocurrio un error inesperado",
                };
            }
        }



        public async Task<ServiceResponse<Service>> SetStateAsync(int serviceId, bool state)
        {
            var response = new ServiceResponse<Service>();

            var existingservice = await _serviceRepository.GetByIdAsync(serviceId);
            if (existingservice == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "El Servicio no existe";
                return response;
            }

            var repoResponse = await _serviceRepository.SetStateAsync(serviceId, state);

            if (repoResponse.Data == null)
            {
                response.Data = null;
                response.IsSuccess = false;
                response.MessageCode = MessageCodes.ErrorValidation;
                response.Message = "No se pudo actualizar el estado del Servicio";
                return response;
            }

            response.Data = repoResponse.Data;
            response.IsSuccess = true;
            response.MessageCode = MessageCodes.Success;
            response.Message = state ? "Servicio activo" : "Servicio inactivo";

            return response;
        }


    }
}
