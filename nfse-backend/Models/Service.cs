namespace nfse_backend.Models
{
    public class Service
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public int Quantity { get; set; }
        public decimal IBSRate { get; set; }
    }
}
