namespace nfse_backend.Models
{
    public class Prestador
    {
        public string Cnpj { get; set; } = string.Empty;
        public string RazaoSocial { get; set; } = string.Empty;
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
        public string? Cep { get; set; }
        public string? InscricaoMunicipal { get; set; }
        public string? CodigoMunicipio { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
    }
}
