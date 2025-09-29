using System;

namespace nfse_backend.Services.WebService
{
    /// <summary>
    /// Exceção lançada quando a SEFAZ está indisponível após múltiplas tentativas
    /// </summary>
    public class SefazIndisponivelException : Exception
    {
        public string? UF { get; set; }
        public string? TipoOperacao { get; set; }
        public DateTime DataHoraFalha { get; set; }
        public int TentativasRealizadas { get; set; }

        public SefazIndisponivelException() : base()
        {
            DataHoraFalha = DateTime.Now;
        }

        public SefazIndisponivelException(string message) : base(message)
        {
            DataHoraFalha = DateTime.Now;
        }

        public SefazIndisponivelException(string message, Exception innerException) : base(message, innerException)
        {
            DataHoraFalha = DateTime.Now;
        }

        public SefazIndisponivelException(string message, string uf, string tipoOperacao, int tentativas) : base(message)
        {
            UF = uf;
            TipoOperacao = tipoOperacao;
            TentativasRealizadas = tentativas;
            DataHoraFalha = DateTime.Now;
        }
    }
}
