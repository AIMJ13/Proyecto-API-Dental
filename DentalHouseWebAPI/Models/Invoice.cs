namespace DentalHouseWebAPI.Models
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
