namespace DentalHouseWebAPI.Models
{
    public class SaleTransaction
    {
        public Sale Master { get; set; } = new();
        public IEnumerable<SaleDetail> Details { get; set; } = Enumerable.Empty<SaleDetail>();
    }
}
