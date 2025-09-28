using Microsoft.Extensions.Logging;

namespace nfse_backend.Services.Monitoramento
{
    public class AuditoriaService
    {
        private readonly ILogger<AuditoriaService> _logger;

        public AuditoriaService(ILogger<AuditoriaService> logger)
        {
            _logger = logger;
        }

        public void LogEmissaoNFe(string chaveAcesso, string cnpjEmitente, string cnpjDestinatario, decimal valorTotal, string status)
        {
            _logger.LogInformation("AUDITORIA_NFE_EMISSAO: {ChaveAcesso} | {CnpjEmitente} | {CnpjDestinatario} | {ValorTotal:C} | {Status}", 
                chaveAcesso, MascararCNPJ(cnpjEmitente), MascararCNPJ(cnpjDestinatario), valorTotal, status);
        }

        public void LogCancelamentoNFe(string chaveAcesso, string cnpjEmitente, string justificativa, string protocolo)
        {
            _logger.LogWarning("AUDITORIA_NFE_CANCELAMENTO: {ChaveAcesso} | {CnpjEmitente} | {Protocolo} | Justificativa: {Justificativa}", 
                chaveAcesso, MascararCNPJ(cnpjEmitente), protocolo, justificativa);
        }

        public void LogCartaCorrecao(string chaveAcesso, string cnpjEmitente, string textoCorrecao, int sequencia)
        {
            _logger.LogInformation("AUDITORIA_NFE_CCE: {ChaveAcesso} | {CnpjEmitente} | Seq: {Sequencia} | Correção: {TextoCorrecao}", 
                chaveAcesso, MascararCNPJ(cnpjEmitente), sequencia, textoCorrecao);
        }

        public void LogInutilizacao(string cnpjEmitente, string uf, int serie, int numeroInicial, int numeroFinal, string justificativa)
        {
            _logger.LogWarning("AUDITORIA_NFE_INUTILIZACAO: {CnpjEmitente} | {UF} | Série: {Serie} | Números: {NumeroInicial}-{NumeroFinal} | Justificativa: {Justificativa}", 
                MascararCNPJ(cnpjEmitente), uf, serie, numeroInicial, numeroFinal, justificativa);
        }

        public void LogErroSefaz(string operacao, string uf, string erro, string dadosRequisicao = "")
        {
            _logger.LogError("AUDITORIA_ERRO_SEFAZ: {Operacao} | {UF} | Erro: {Erro} | Dados: {DadosRequisicao}", 
                operacao, uf, erro, dadosRequisicao);
        }

        public void LogAcessoCertificado(string cnpjEmpresa, string tipoCertificado, bool sucesso, string detalhes = "")
        {
            _logger.LogInformation("AUDITORIA_CERTIFICADO: {CnpjEmpresa} | {TipoCertificado} | Sucesso: {Sucesso} | {Detalhes}", 
                MascararCNPJ(cnpjEmpresa), tipoCertificado, sucesso, detalhes);
        }

        public void RegistrarAuditoria(string operacao, string detalhes, string usuario = "Sistema")
        {
            _logger.LogInformation("AUDITORIA: {Operacao} | {Usuario} | {Detalhes}", operacao, usuario, detalhes);
        }

        private string MascararCNPJ(string cnpj)
        {
            if (string.IsNullOrEmpty(cnpj) || cnpj.Length != 14)
                return cnpj;

            // Mascarar CNPJ: 12.345.***/**01-99
            return $"{cnpj.Substring(0, 2)}.{cnpj.Substring(2, 3)}.***/**{cnpj.Substring(8, 2)}-{cnpj.Substring(12, 2)}";
        }
    }
}
