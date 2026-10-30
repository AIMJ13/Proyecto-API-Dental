using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebAPI.Core.Common;
using WebAPI.Core.Entities;

namespace WebAPI.DataAccess.Interfaces
{
    public interface IInvoiceRepository
    {
        Task<RepositoryResponse<ConcurrentQueue<Invoice>>> GetInvoiceQueue();

        Task<RepositoryResponse<Invoice>> ToPrint();
    }
}
