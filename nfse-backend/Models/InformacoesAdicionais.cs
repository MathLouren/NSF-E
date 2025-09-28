using System.Text.Json.Serialization;

namespace nfse_backend.Models
{
    public class InformacoesAdicionais
    {
        [JsonPropertyName("formaPagamento")]
        public string FormaPagamento { get; set; } = string.Empty;
        
        [JsonPropertyName("observacoes")]
        public string? Observacoes { get; set; }
        
        [JsonPropertyName("municipioPrestador")]
        public string? MunicipioPrestador { get; set; }
        
        [JsonPropertyName("codigoTributacaoMunicipal")]
        public string? CodigoTributacaoMunicipal { get; set; }
        
        [JsonPropertyName("protocoloEnvio")]
        public string? ProtocoloEnvio { get; set; }
    }
}
