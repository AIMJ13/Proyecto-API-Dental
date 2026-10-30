namespace DentalHouseWebAPI.Models
{
    public class SaleDetail
    {
        public int Id { get; set; }
        public int SaleId { get; set; }
        public int ServiceId { get; set; }
        public int Quantity { get; set; }

        public decimal LineTotal { get; set; }
        public decimal SalePrice { get; set; }
    }
}
