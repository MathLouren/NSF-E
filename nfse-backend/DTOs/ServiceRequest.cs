namespace nfse_backend.DTOs
{
    public class ServiceRequest
    {
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public int Quantity { get; set; }
        public decimal IBSRate { get; set; }
    }
}
