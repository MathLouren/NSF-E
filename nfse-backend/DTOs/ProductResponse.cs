namespace nfse_backend.DTOs
{
    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string SKU { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Unit { get; set; } = string.Empty;
        public decimal TotalValue { get; set; } = 0.00M;
        public decimal Discount { get; set; } = 0.00M;
        public string? Category { get; set; }
        public decimal IBSRate { get; set; }
        public decimal? BaseCalculoIss { get; set; }
        public decimal? ValorIss { get; set; }
    }
}
