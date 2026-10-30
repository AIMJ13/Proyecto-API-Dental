using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Core.Entities
{
    public class SaleDetail
    {
        public int SaleDetailId { get; set; }
        public int SaleId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }
        public string? ServiceName { get; set; }
        public string? Cost { get; set; }
        public decimal SubTotal
        {
            get
            {
                decimal.TryParse(Cost, out var cost);
                return cost * Quantity;
            }
        }
    }
}
