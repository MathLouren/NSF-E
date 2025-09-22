namespace nfse_backend.Models
{
    public class Tomador
    {
        public string CpfCnpj { get; set; } = string.Empty;
        public string NomeRazaoSocial { get; set; } = string.Empty;
        public string? Logradouro { get; set; }
        public string? Numero { get; set; }
        public string? Bairro { get; set; }
        public string? Municipio { get; set; }
        public string? Uf { get; set; }
        public string? Cep { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
    }
}
