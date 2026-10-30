using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Common
{
    public class RepositoryResponse<T>
    {
        public T? Data { get; set; }
        
        public int OperationStatusCode { get; set; }
       
        public string Message { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
    }
}
