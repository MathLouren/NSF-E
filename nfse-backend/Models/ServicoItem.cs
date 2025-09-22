namespace nfse_backend.Models
{
    public class ServicoItem
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Quantidade { get; set; }
        public decimal ValorUnitario { get; set; }
        public decimal ValorTotal { get; set; }
        public decimal Desconto { get; set; } = 0.00M;
        public decimal AliquotaIbs { get; set; }
        public bool IssRetido { get; set; }
        public decimal? BaseCalculoIss { get; set; }
        public decimal? ValorIss { get; set; }
        public string Unidade { get; set; } = string.Empty;
        public string? Categoria { get; set; }
    }
}
