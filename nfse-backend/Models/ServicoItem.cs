using System.Text.Json.Serialization;

namespace nfse_backend.Models
{
    public class ServicoItem
    {
        [JsonPropertyName("codigo")]
        public string Codigo { get; set; } = string.Empty;
        
        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;
        
        [JsonPropertyName("quantidade")]
        public int Quantidade { get; set; }
        
        [JsonPropertyName("valorUnitario")]
        public decimal ValorUnitario { get; set; }
        
        [JsonPropertyName("valorTotal")]
        public decimal ValorTotal { get; set; }
        
        [JsonPropertyName("desconto")]
        public decimal Desconto { get; set; } = 0.00M;
        
        [JsonPropertyName("aliquotaIbs")]
        public decimal AliquotaIbs { get; set; }
        
        [JsonPropertyName("issRetido")]
        public bool IssRetido { get; set; }
        
        [JsonPropertyName("baseCalculoIss")]
        public decimal? BaseCalculoIss { get; set; }
        
        [JsonPropertyName("valorIss")]
        public decimal? ValorIss { get; set; }
        
        [JsonPropertyName("unidade")]
        public string Unidade { get; set; } = string.Empty;
        
        [JsonPropertyName("categoria")]
        public string? Categoria { get; set; }
    }
}
