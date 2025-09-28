using System.Text.Json.Serialization;

namespace nfse_backend.Models
{
    public class Prestador
    {
        [JsonPropertyName("cnpj")]
        public string Cnpj { get; set; } = string.Empty;
        
        [JsonPropertyName("razaoSocial")]
        public string RazaoSocial { get; set; } = string.Empty;
        
        [JsonPropertyName("logradouro")]
        public string? Logradouro { get; set; }
        
        [JsonPropertyName("numero")]
        public string? Numero { get; set; }
        
        [JsonPropertyName("bairro")]
        public string? Bairro { get; set; }
        
        [JsonPropertyName("municipio")]
        public string? Municipio { get; set; }
        
        [JsonPropertyName("uf")]
        public string? Uf { get; set; }
        
        [JsonPropertyName("cep")]
        public string? Cep { get; set; }
        
        [JsonPropertyName("inscricaoMunicipal")]
        public string? InscricaoMunicipal { get; set; }
        
        [JsonPropertyName("codigoMunicipio")]
        public string? CodigoMunicipio { get; set; }
        
        [JsonPropertyName("telefone")]
        public string? Telefone { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}
