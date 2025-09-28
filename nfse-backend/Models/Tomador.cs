using System.Text.Json.Serialization;

namespace nfse_backend.Models
{
    public class Tomador
    {
        [JsonPropertyName("cpfCnpj")]
        public string CpfCnpj { get; set; } = string.Empty;
        
        [JsonPropertyName("nomeRazaoSocial")]
        public string NomeRazaoSocial { get; set; } = string.Empty;
        
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
        
        [JsonPropertyName("telefone")]
        public string? Telefone { get; set; }
        
        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }
}
