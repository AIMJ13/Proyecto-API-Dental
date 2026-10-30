using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebAPI.Business.DTOs
{
    public class SaleDetailDto
    {
        public required int Id { get; set; }
        public required int SaleId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal SalePrice { get; set; }
        public decimal LineTotal { get; set; }
    }
}
