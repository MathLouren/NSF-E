namespace nfse_backend.Models
{
    public class InformacoesAdicionais
    {
        public string FormaPagamento { get; set; } = string.Empty;
        public string? Observacoes { get; set; }
        public string? MunicipioPrestador { get; set; }
        public string? CodigoTributacaoMunicipal { get; set; }
        public string? ProtocoloEnvio { get; set; }
    }
}
