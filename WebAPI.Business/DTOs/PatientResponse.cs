namespace DentalHouseWebAPI.DTOs
{
    public class PatientResponse<T>
    {
        public T? Data { get; set; }
        public bool IsSucces { get; set; }
        public string? MessageCodes { get; set; }
        public string? Message { get; set; }
        public int? OperactionStatusCode { get; set; }
    }
}
