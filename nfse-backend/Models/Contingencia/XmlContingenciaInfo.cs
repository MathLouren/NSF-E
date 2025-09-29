using System;

namespace nfse_backend.Models.Contingencia
{
    /// <summary>
    /// Informações sobre um XML armazenado em contingência
    /// </summary>
    public class XmlContingenciaInfo
    {
        public string ChaveAcesso { get; set; } = "";
        public string CaminhoArquivo { get; set; } = "";
        public string UF { get; set; } = "";
        public bool Homologacao { get; set; }
        public DateTime DataCriacao { get; set; }
        public int TentativasReenvio { get; set; }
        public DateTime? UltimaTentativa { get; set; }
        public string? UltimoErro { get; set; }
        public string TipoContingencia { get; set; } = ""; // EPEC, FSDA, SVCAN
        public long TamanhoArquivo { get; set; }
        public string CnpjEmitente { get; set; } = "";
    }

    /// <summary>
    /// Resultado do processamento de reenvio de contingência
    /// </summary>
    public class ResultadoReenvioContingencia
    {
        public bool Sucesso { get; set; }
        public string Protocolo { get; set; } = "";
        public string CodigoStatus { get; set; } = "";
        public string Mensagem { get; set; } = "";
        public DateTime DataProcessamento { get; set; } = DateTime.Now;
    }
}
