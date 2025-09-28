using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using nfse_backend.Models.NFe;
using nfse_backend.Services.Xml;
using nfse_backend.Services.Validation;
using nfse_backend.Services.Calculation;
using nfse_backend.Services.Audit;
using nfse_backend.Services.Certificado;
using nfse_backend.Services.WebService;
using nfse_backend.Services.Monitoramento;

namespace nfse_backend.Services.NFe
{
    /// <summary>
    /// Serviço principal para NF-e 2026
    /// Integra todos os componentes necessários para processamento completo
    /// </summary>
    public class NFe2026Service
    {
        private readonly XmlGenerator2026Service _xmlGenerator;
        private readonly NFe2026ValidationService _validationService;
        private readonly NFe2026CalculationService _calculationService;
        private readonly NFe2026AuditService _auditService;
        private readonly CertificadoDigitalService _certificadoService;
        private readonly SefazWebServiceClient _webServiceClient;
        private readonly MonitoramentoService _monitoramentoService;

        public NFe2026Service(
            XmlGenerator2026Service xmlGenerator,
            NFe2026ValidationService validationService,
            NFe2026CalculationService calculationService,
            NFe2026AuditService auditService,
            CertificadoDigitalService certificadoService,
            SefazWebServiceClient webServiceClient,
            MonitoramentoService monitoramentoService)
        {
            _xmlGenerator = xmlGenerator;
            _validationService = validationService;
            _calculationService = calculationService;
            _auditService = auditService;
            _certificadoService = certificadoService;
            _webServiceClient = webServiceClient;
            _monitoramentoService = monitoramentoService;
        }

        #region Processamento Principal

        public async Task<NFe2026Result> ProcessarNFe2026Async(NFe2026 nfe)
        {
            var result = new NFe2026Result();

            try
            {
                _monitoramentoService.RegistrarEvento("NFe2026_Processamento", "Iniciando processamento NF-e 2026", nfe.ChaveAcesso);

                // 1. Calcular totais
                _calculationService.CalcularTotaisNFe2026(nfe);

                // 2. Validar NF-e
                var validacao = _validationService.ValidarNFe2026(nfe);
                if (!validacao.SchemaValido || !validacao.RegrasNegocioValidas || !validacao.CodigosTributariosValidos)
                {
                    result.Sucesso = false;
                    result.Mensagem = "Erro na validação da NF-e 2026";
                    result.Erros = validacao.ErrosValidacao;
                    return result;
                }

                // 3. Gerar XML
                var xml = _xmlGenerator.GenerateNFe2026Xml(nfe);
                result.XmlGerado = xml;

                // 4. Assinar XML
                var xmlAssinado = _certificadoService.AssinarXml(xml);
                result.XmlAssinado = xmlAssinado;

                // 5. Auditoria
                _auditService.AuditarNFe2026(nfe);

                // 6. Enviar para SEFAZ
                var retornoEnvio = await _webServiceClient.EnviarLoteNFe(
                    CriarLoteNFe2026(xmlAssinado),
                    nfe.Emit.EnderEmit.UF,
                    nfe.Ide.tpAmb == 2 // Homologação
                );

                // 7. Processar retorno
                result = ProcessarRetornoEnvio(retornoEnvio, nfe);

                _monitoramentoService.RegistrarEvento("NFe2026_Processamento", 
                    result.Sucesso ? "Processamento concluído com sucesso" : "Erro no processamento", 
                    nfe.ChaveAcesso);

                return result;
            }
            catch (Exception ex)
            {
                result.Sucesso = false;
                result.Mensagem = $"Erro no processamento: {ex.Message}";
                result.Erros = new List<string> { ex.Message };

                _monitoramentoService.RegistrarEvento("NFe2026_Erro", ex.Message, nfe.ChaveAcesso);
                return result;
            }
        }

        #endregion

        #region Processamento de Eventos

        public async Task<NFe2026Result> ProcessarEventoFiscalAsync(NFe2026 nfe, EventoFiscal evento)
        {
            var result = new NFe2026Result();

            try
            {
                // Validar evento
                if (!ValidarEventoFiscal(evento))
                {
                    result.Sucesso = false;
                    result.Mensagem = "Evento fiscal inválido";
                    return result;
                }

                // Adicionar evento à NF-e
                nfe.EventosFiscais.Add(evento);

                // Gerar XML do evento
                var xmlEvento = GerarXmlEventoFiscal(nfe, evento);

                // Assinar XML do evento
                var xmlEventoAssinado = _certificadoService.AssinarXml(xmlEvento);

                // Enviar evento para SEFAZ
                var retornoEvento = await _webServiceClient.EnviarEventoNFe(
                    xmlEventoAssinado,
                    nfe.Emit.EnderEmit.UF,
                    nfe.Ide.tpAmb == 2
                );

                // Processar retorno do evento
                result = ProcessarRetornoEvento(retornoEvento, evento);

                return result;
            }
            catch (Exception ex)
            {
                result.Sucesso = false;
                result.Mensagem = $"Erro no processamento do evento: {ex.Message}";
                return result;
            }
        }

        #endregion

        #region Consultas

        public async Task<NFe2026Result> ConsultarStatusNFeAsync(string chaveAcesso, string uf, bool homologacao)
        {
            var result = new NFe2026Result();

            try
            {
                var retornoConsulta = await _webServiceClient.ConsultarProtocoloNFe(chaveAcesso, uf, homologacao);
                
                result.Sucesso = true;
                result.Mensagem = "Consulta realizada com sucesso";
                result.DadosRetorno = retornoConsulta;

                return result;
            }
            catch (Exception ex)
            {
                result.Sucesso = false;
                result.Mensagem = $"Erro na consulta: {ex.Message}";
                return result;
            }
        }

        public async Task<NFe2026Result> ConsultarDistribuicaoDFeAsync(string cnpj, string ultimoNSU, bool homologacao)
        {
            var result = new NFe2026Result();

            try
            {
                var retornoDistribuicao = await _webServiceClient.ConsultarDistribuicaoDFe(cnpj, "BR", ultimoNSU, homologacao);
                
                result.Sucesso = true;
                result.Mensagem = "Consulta de distribuição realizada com sucesso";
                result.DadosRetorno = retornoDistribuicao;

                return result;
            }
            catch (Exception ex)
            {
                result.Sucesso = false;
                result.Mensagem = $"Erro na consulta de distribuição: {ex.Message}";
                return result;
            }
        }

        #endregion

        #region Métodos Auxiliares

        private string CriarLoteNFe2026(string xmlAssinado)
        {
            // Criar lote específico para NF-e 2026
            var lote = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<enviNFe xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""4.00"">
    <idLote>{DateTime.Now:yyyyMMddHHmmss}</idLote>
    <indSinc>1</indSinc>
    <NFe>
        {xmlAssinado}
    </NFe>
</enviNFe>";

            return lote;
        }

        private NFe2026Result ProcessarRetornoEnvio(string retornoEnvio, NFe2026 nfe)
        {
            var result = new NFe2026Result();

            try
            {
                // Parse do retorno XML
                var xmlRetorno = System.Xml.Linq.XDocument.Parse(retornoEnvio);
                var namespaceRet = "http://www.portalfiscal.inf.br/nfe";

                var cStat = xmlRetorno.Descendants(namespaceRet + "cStat").FirstOrDefault()?.Value;
                var xMotivo = xmlRetorno.Descendants(namespaceRet + "xMotivo").FirstOrDefault()?.Value;
                var nRec = xmlRetorno.Descendants(namespaceRet + "nRec").FirstOrDefault()?.Value;

                if (cStat == "103") // Lote recebido com sucesso
                {
                    result.Sucesso = true;
                    result.Mensagem = "Lote recebido com sucesso";
                    result.NumeroRecibo = nRec;
                    result.Status = "LOTE_RECEBIDO";

                    // Atualizar NF-e
                    nfe.Status = "LOTE_RECEBIDO";
                    nfe.Protocolo = nRec;
                }
                else
                {
                    result.Sucesso = false;
                    result.Mensagem = xMotivo;
                    result.Erros = new List<string> { $"Código: {cStat} - {xMotivo}" };
                    result.Status = "REJEITADO";
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Sucesso = false;
                result.Mensagem = $"Erro ao processar retorno: {ex.Message}";
                return result;
            }
        }

        private NFe2026Result ProcessarRetornoEvento(string retornoEvento, EventoFiscal evento)
        {
            var result = new NFe2026Result();

            try
            {
                var xmlRetorno = System.Xml.Linq.XDocument.Parse(retornoEvento);
                var namespaceRet = "http://www.portalfiscal.inf.br/nfe";

                var cStat = xmlRetorno.Descendants(namespaceRet + "cStat").FirstOrDefault()?.Value;
                var xMotivo = xmlRetorno.Descendants(namespaceRet + "xMotivo").FirstOrDefault()?.Value;
                var nProt = xmlRetorno.Descendants(namespaceRet + "nProt").FirstOrDefault()?.Value;

                if (cStat == "135") // Evento registrado com sucesso
                {
                    result.Sucesso = true;
                    result.Mensagem = "Evento registrado com sucesso";
                    result.ProtocoloEvento = nProt;
                    result.Status = "EVENTO_REGISTRADO";

                    // Atualizar evento
                    evento.StatusEvento = "REGISTRADO";
                    evento.ProtocoloEvento = nProt;
                }
                else
                {
                    result.Sucesso = false;
                    result.Mensagem = xMotivo;
                    result.Erros = new List<string> { $"Código: {cStat} - {xMotivo}" };
                    result.Status = "EVENTO_REJEITADO";
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Sucesso = false;
                result.Mensagem = $"Erro ao processar retorno do evento: {ex.Message}";
                return result;
            }
        }

        private bool ValidarEventoFiscal(EventoFiscal evento)
        {
            // Validar campos obrigatórios
            if (string.IsNullOrEmpty(evento.TipoEvento))
                return false;

            if (string.IsNullOrEmpty(evento.DescricaoEvento))
                return false;

            if (evento.DataEvento > DateTime.Now)
                return false;

            // Validar tipo de evento
            var tiposValidos = new[] { "CREDITO_PRESUMIDO", "PERDA_ROUBO", "CANCELAMENTO", "TRANSFERENCIA_CREDITO" };
            if (!tiposValidos.Contains(evento.TipoEvento))
                return false;

            return true;
        }

        private string GerarXmlEventoFiscal(NFe2026 nfe, EventoFiscal evento)
        {
            // Gerar XML do evento fiscal conforme layout 2026
            var xmlEvento = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<evento xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""1.00"">
    <infEvento Id=""ID{evento.TipoEvento}{nfe.ChaveAcesso}"">
        <cOrgao>{nfe.Ide.cUF}</cOrgao>
        <tpAmb>{nfe.Ide.tpAmb}</tpAmb>
        <CNPJ>{nfe.Emit.CNPJ}</CNPJ>
        <chNFe>{nfe.ChaveAcesso}</chNFe>
        <dhEvento>{evento.DataEvento:yyyy-MM-ddTHH:mm:sszzz}</dhEvento>
        <tpEvento>{evento.TipoEvento}</tpEvento>
        <nSeqEvento>1</nSeqEvento>
        <verEvento>1.00</verEvento>
        <detEvento versao=""1.00"">
            <descEvento>{evento.DescricaoEvento}</descEvento>
            <xJust>{evento.Justificativa}</xJust>
        </detEvento>
    </infEvento>
</evento>";

            return xmlEvento;
        }

        #endregion
    }

    #region Classes de Resultado

    public class NFe2026Result
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; }
        public List<string> Erros { get; set; } = new List<string>();
        public string Status { get; set; }
        public string XmlGerado { get; set; }
        public string XmlAssinado { get; set; }
        public string NumeroRecibo { get; set; }
        public string ProtocoloEvento { get; set; }
        public object DadosRetorno { get; set; }
        public DateTime DataProcessamento { get; set; } = DateTime.UtcNow;
    }

    #endregion
}
