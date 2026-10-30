using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.Business.Interfaces
{
    public interface IInvoiceService
    {
        Task<ServiceResponse<List<Invoice>>> InvoiceQueue();
        Task<ServiceResponse<Invoice>> ToPrint();
    }
}
