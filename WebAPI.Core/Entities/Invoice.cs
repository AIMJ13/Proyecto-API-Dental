using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class Invoice
    {
        public int InvoiceId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int SaleId { get; set; }
       
        public decimal TotalAmount { get; set; }
        public bool IsPrinted { get; set; }
    }
}
