namespace nfse_backend.Models
{
    public class Company
    {
        public Guid Id { get; set; }
        public string CNPJ { get; set; } = string.Empty;
        public string LegalName { get; set; } = string.Empty;
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
        public decimal DefaultIbsRate { get; set; }
    }
}
