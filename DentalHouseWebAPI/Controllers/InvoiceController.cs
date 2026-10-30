using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Business.DTOs;
using WebAPI.Business.Interfaces;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace DentalHouseWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceService _invoiceService;

        public InvoiceController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
        }
        [HttpGet("PrintQueue")]
        public async Task<IActionResult> PrintQueue(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10)
        {
            var serviceResponse = await _invoiceService.InvoiceQueue();
            if (serviceResponse.IsSuccess && serviceResponse.Data != null)
            {
                var total = serviceResponse.Data.Count;
                var paged = serviceResponse.Data.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

                var response = new
                {
                    pageNumber,
                    pageSize,
                    totalRecords = total,
                    totalPages = (int)Math.Ceiling(total / (double)pageSize),
                    data = paged
                };

                return Ok(response);
            }
            var unSuccessfulResponse = new UnsuccessfulResponseDTO();

            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unSuccessfulResponse.Code = "200";
                    unSuccessfulResponse.Message = "No se encontraron registros";
                    unSuccessfulResponse.Details = new { info = "No hay facturas que imprimir" };

                    return Ok(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno en la aplicacion" };

                    return StatusCode(500, unSuccessfulResponse);
            }
        }
        
        [HttpGet("ToPrint")]
        public async Task<IActionResult> ToPrint()
        {
            var serviceResponse = await _invoiceService.ToPrint();
            if (serviceResponse.IsSuccess)
            {
                var response = new ApiResponse<Invoice>
                {
                    Data = serviceResponse.Data,
                    Meta = new { message = serviceResponse.Message }
                };
                return Ok(response);
            }
            var unSuccessfulResponse = new UnsuccessfulResponseDTO();
            switch (serviceResponse.MessageCode)
            {
                case MessageCodes.NoData:
                    unSuccessfulResponse.Code = "200";
                    unSuccessfulResponse.Message = "No se encontraron registros";
                    unSuccessfulResponse.Details = new { info = "No hay facturas que imprimir" };
                    return Ok(unSuccessfulResponse);

                default:
                    unSuccessfulResponse.Code = "500";
                    unSuccessfulResponse.Message = "Ocurrio un error inesperado";
                    unSuccessfulResponse.Details = new { info = "Error interno en la aplicacion" };
                    return StatusCode(500, unSuccessfulResponse);
            }
        }
    }
}
