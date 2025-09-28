using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using nfse_backend.Models.NFe;
using NFeDoc = nfse_backend.Models.NFe.NFe;
using nfse_backend.Services.Armazenamento;
using nfse_backend.Services.Certificado;
using nfse_backend.Services.Configuracao;
using nfse_backend.Services.Eventos;
using nfse_backend.Services.Impostos;
using nfse_backend.Services.Pdf;
using nfse_backend.Services.WebService;
using nfse_backend.Services.Xml;

namespace nfse_backend.Services.NotaFiscal
{
    public class NFeService
    {
        private readonly ILogger<NFeService> _logger;
        private readonly XmlGeneratorService _xmlGenerator;
        private readonly XmlValidationService _xmlValidation;
        private readonly CertificadoDigitalService _certificadoService;
        private readonly SefazWebServiceClient _webServiceClient;
        private readonly CalculoImpostosService _calculoImpostos;
        private readonly EventosNFeService _eventosService;
        private readonly DanfeService _danfeService;
        private readonly ConfiguracaoNFeService _configuracaoService;
        private readonly ArmazenamentoSeguroService _armazenamentoService;

        public NFeService(
            ILogger<NFeService> logger,
            XmlGeneratorService xmlGenerator,
            XmlValidationService xmlValidation,
            CertificadoDigitalService certificadoService,
            SefazWebServiceClient webServiceClient,
            CalculoImpostosService calculoImpostos,
            EventosNFeService eventosService,
            DanfeService danfeService,
            ConfiguracaoNFeService configuracaoService,
            ArmazenamentoSeguroService armazenamentoService)
        {
            _logger = logger;
            _xmlGenerator = xmlGenerator;
            _xmlValidation = xmlValidation;
            _certificadoService = certificadoService;
            _webServiceClient = webServiceClient;
            _calculoImpostos = calculoImpostos;
            _eventosService = eventosService;
            _danfeService = danfeService;
            _configuracaoService = configuracaoService;
            _armazenamentoService = armazenamentoService;
        }

        public async Task<NFeResult> EmitirNFe(NFeDoc nfe)
        {
            var result = new NFeResult();

            try
            {
                _logger.LogInformation($"Iniciando emissão de NF-e para CNPJ: {nfe.Emit.CNPJ}");

                // 1. Obter configurações da empresa
                var configEmpresa = _configuracaoService.ObterConfiguracaoEmpresa(nfe.Emit.CNPJ);
                var ambiente = _configuracaoService.ObterAmbienteAtual();

                // 2. Carregar certificado digital
                (byte[] certBytes, string senha) = await _armazenamentoService.CarregarCertificadoSeguro(nfe.Emit.CNPJ);
                if (certBytes != null && certBytes.Length > 0)
                {
                    _certificadoService.CarregarCertificadoA1Bytes(certBytes, senha);
                }
                else
                {
                    _certificadoService.CarregarCertificadoA1(configEmpresa.CertificadoPath ?? "", senha);
                }

                // 3. Configurar ambiente
                nfe.Ide.tpAmb = (int)ambiente;

                // 4. Gerar número sequencial
                nfe.Ide.nNF = configEmpresa.ProximoNumeroNFe;
                nfe.Ide.serie = configEmpresa.SerieNFe;

                // 5. Gerar código numérico aleatório
                var random = new Random();
                nfe.Ide.cNF = random.Next(10000000, 99999999).ToString();

                // 6. Calcular impostos
                foreach (var det in nfe.Det)
                {
                    det.Imposto.ICMS = _calculoImpostos.CalcularICMS(
                        det.Prod, 
                        nfe.Emit.EnderEmit.UF, 
                        nfe.Dest.EnderDest.UF,
                        configEmpresa.CRT,
                        nfe.Ide.indFinal == 1,
                        nfe.Dest.indIEDest != 9
                    );

                    det.Imposto.IPI = _calculoImpostos.CalcularIPI(
                        det.Prod,
                        det.Prod.NCM,
                        configEmpresa.CRT == 0 // Industria se não for Simples Nacional
                    );

                    det.Imposto.PIS = _calculoImpostos.CalcularPIS(
                        det.Prod,
                        configEmpresa.CRT,
                        configEmpresa.RegimeTributario == "LUCRO_REAL"
                    );

                    det.Imposto.COFINS = _calculoImpostos.CalcularCOFINS(
                        det.Prod,
                        configEmpresa.CRT,
                        configEmpresa.RegimeTributario == "LUCRO_REAL"
                    );

                    // Calcular tributo aproximado
                    det.Imposto.vTotTrib = _calculoImpostos.CalcularTributoAproximado(
                        det.Prod.vProd,
                        det.Prod.NCM,
                        nfe.Dest.EnderDest.UF
                    );
                }

                // 7. Calcular totais
                _calculoImpostos.CalcularTotaisNFe(nfe);

                // 8. Gerar XML
                var xml = _xmlGenerator.GenerateNFeXml(nfe);

                // 9. Validar XML contra XSD
                List<string> errosValidacao;
                if (!_xmlValidation.ValidateNFeXml(xml, out errosValidacao))
                {
                    result.Sucesso = false;
                    result.Mensagem = "Erro na validação do XML";
                    result.Erros = errosValidacao;
                    return result;
                }

                // 10. Assinar XML
                var xmlAssinado = _certificadoService.AssinarXml(xml);

                // 11. Criar lote
                var xmlLote = CriarLoteNFe(xmlAssinado);

                // 12. Enviar para SEFAZ
                var retornoEnvio = await _webServiceClient.EnviarLoteNFe(
                    xmlLote,
                    nfe.Emit.EnderEmit.UF,
                    ambiente == AmbienteNFe.Homologacao
                );

                // 13. Processar retorno do envio
                var (numeroRecibo, statusEnvio, motivoEnvio) = ProcessarRetornoEnvio(retornoEnvio);

                if (string.IsNullOrEmpty(numeroRecibo))
                {
                    result.Sucesso = false;
                    result.Mensagem = $"Erro no envio: {motivoEnvio}";
                    result.CodigoStatus = statusEnvio;
                    
                    // Salvar XML rejeitado
                    await _armazenamentoService.SalvarXmlRejeitado(
                        xmlAssinado,
                        DateTime.Now.Ticks.ToString(),
                        nfe.Emit.CNPJ,
                        motivoEnvio
                    );
                    
                    return result;
                }

                // 14. Consultar autorização
                await Task.Delay(3000); // Aguardar processamento

                var retornoConsulta = await _webServiceClient.ConsultarRecibo(
                    numeroRecibo,
                    nfe.Emit.EnderEmit.UF,
                    ambiente == AmbienteNFe.Homologacao
                );

                // 15. Processar retorno da autorização
                var (protocolo, statusAutorizacao, motivoAutorizacao, xmlProtocolo) = 
                    ProcessarRetornoAutorizacao(retornoConsulta);

                if (statusAutorizacao == "100") // Autorizado
                {
                    result.Sucesso = true;
                    result.Protocolo = protocolo ?? string.Empty;
                    result.ChaveAcesso = nfe.ChaveAcesso ?? string.Empty;
                    result.NumeroNFe = nfe.Ide.nNF;
                    result.Serie = nfe.Ide.serie;
                    result.DataAutorizacao = DateTime.Now;
                    result.XmlAutorizado = xmlProtocolo ?? string.Empty;
                    
                    // Salvar XML autorizado
                    await _armazenamentoService.SalvarXmlAutorizado(
                        xmlProtocolo ?? xmlAssinado,
                        nfe.ChaveAcesso ?? string.Empty,
                        nfe.Emit.CNPJ ?? string.Empty
                    );

                    // Gerar e salvar PDF
                    var pdfBytes = _danfeService.GenerateDanfe(nfe);
                    await _armazenamentoService.SalvarPDF(
                        pdfBytes,
                        nfe.ChaveAcesso ?? string.Empty,
                        nfe.Emit.CNPJ ?? string.Empty
                    );

                    // Atualizar próximo número
                    configEmpresa.ProximoNumeroNFe++;
                    _configuracaoService.SalvarConfiguracaoEmpresa(configEmpresa);

                    _logger.LogInformation($"NF-e autorizada: {nfe.ChaveAcesso}");
                }
                else
                {
                    result.Sucesso = false;
                    result.Mensagem = $"NF-e não autorizada: {motivoAutorizacao}";
                    result.CodigoStatus = statusAutorizacao;
                    
                    // Salvar XML rejeitado
                    await _armazenamentoService.SalvarXmlRejeitado(
                        xmlAssinado,
                        numeroRecibo,
                        nfe.Emit.CNPJ,
                        motivoAutorizacao
                    );
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao emitir NF-e");
                result.Sucesso = false;
                result.Mensagem = $"Erro ao emitir NF-e: {ex.Message}";
                return result;
            }
        }

        public async Task<EventoResult> EnviarCartaCorrecao(
            string chaveAcesso,
            string cnpjEmitente,
            string textoCorrecao,
            int sequenciaEvento = 1)
        {
            try
            {
                var ambiente = _configuracaoService.ObterAmbienteAtual();
                var uf = chaveAcesso.Substring(0, 2);
                
                var retorno = await _eventosService.EnviarCartaCorrecao(
                    chaveAcesso,
                    cnpjEmitente,
                    textoCorrecao,
                    sequenciaEvento,
                    ObterUFPorCodigo(int.Parse(uf)),
                    ambiente == AmbienteNFe.Homologacao
                );

                var eventoRetorno = _eventosService.ProcessarRetornoEvento(retorno);

                return new EventoResult
                {
                    Sucesso = eventoRetorno.Autorizado,
                    Protocolo = eventoRetorno.NProt,
                    Mensagem = eventoRetorno.XMotivo,
                    DataEvento = DateTime.Parse(eventoRetorno.DhRegEvento ?? DateTime.Now.ToString())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao enviar carta de correção");
                return new EventoResult
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<EventoResult> CancelarNFe(
            string chaveAcesso,
            string protocolo,
            string cnpjEmitente,
            string justificativa)
        {
            try
            {
                var ambiente = _configuracaoService.ObterAmbienteAtual();
                var uf = chaveAcesso.Substring(0, 2);
                
                var retorno = await _eventosService.CancelarNFe(
                    chaveAcesso,
                    protocolo,
                    cnpjEmitente,
                    justificativa,
                    ObterUFPorCodigo(int.Parse(uf)),
                    ambiente == AmbienteNFe.Homologacao
                );

                var eventoRetorno = _eventosService.ProcessarRetornoEvento(retorno);

                return new EventoResult
                {
                    Sucesso = eventoRetorno.Autorizado,
                    Protocolo = eventoRetorno.NProt,
                    Mensagem = eventoRetorno.XMotivo,
                    DataEvento = DateTime.Parse(eventoRetorno.DhRegEvento ?? DateTime.Now.ToString())
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cancelar NF-e");
                return new EventoResult
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }

        public async Task<InutilizacaoResult> InutilizarNumeracao(
            string cnpjEmitente,
            string uf,
            int ano,
            int serie,
            int numeroInicial,
            int numeroFinal,
            string justificativa)
        {
            try
            {
                var ambiente = _configuracaoService.ObterAmbienteAtual();
                
                var retorno = await _eventosService.InutilizarNumeracao(
                    cnpjEmitente,
                    uf,
                    ano,
                    serie,
                    numeroInicial,
                    numeroFinal,
                    justificativa,
                    ambiente == AmbienteNFe.Homologacao
                );

                var (status, motivo, protocolo) = ProcessarRetornoInutilizacao(retorno);

                return new InutilizacaoResult
                {
                    Sucesso = status == "102", // Inutilização homologada
                    Protocolo = protocolo ?? string.Empty,
                    Mensagem = motivo,
                    DataInutilizacao = DateTime.Now
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao inutilizar numeração");
                return new InutilizacaoResult
                {
                    Sucesso = false,
                    Mensagem = ex.Message
                };
            }
        }

        private string CriarLoteNFe(string xmlNFe)
        {
            var idLote = DateTime.Now.Ticks.ToString().Substring(0, 15);
            
            var lote = new XDocument(
                new XElement(XNamespace.Get("http://www.portalfiscal.inf.br/nfe") + "enviNFe",
                    new XAttribute("versao", "4.00"),
                    new XElement("idLote", idLote),
                    new XElement("indSinc", "0"), // Processamento assíncrono
                    XElement.Parse(xmlNFe)
                )
            );

            return lote.ToString();
        }

        private (string? numeroRecibo, string status, string motivo) ProcessarRetornoEnvio(string xmlRetorno)
        {
            try
            {
                var doc = XDocument.Parse(xmlRetorno);
                var root = doc.Root;
                if (root == null)
                {
                    return (null, "999", "Retorno inválido da SEFAZ");
                }
                var ns = root.GetDefaultNamespace();

                var infRec = doc.Descendants(ns + "infRec").FirstOrDefault();
                if (infRec != null)
                {
                    var nRec = infRec.Element(ns + "nRec")?.Value;
                    var cStat = infRec.Element(ns + "cStat")?.Value;
                    var xMotivo = infRec.Element(ns + "xMotivo")?.Value;
                    
                    return (nRec, cStat ?? "999", xMotivo ?? string.Empty);
                }

                return (null, "999", "Retorno inválido da SEFAZ");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar retorno do envio");
                return (null, "999", ex.Message ?? string.Empty);
            }
        }

        private (string? protocolo, string status, string motivo, string? xmlProtocolo) 
            ProcessarRetornoAutorizacao(string xmlRetorno)
        {
            try
            {
                var doc = XDocument.Parse(xmlRetorno);
                var root = doc.Root;
                if (root == null)
                {
                    return (null, "999", "Retorno inválido da SEFAZ", null);
                }
                var ns = root.GetDefaultNamespace();

                var protNFe = doc.Descendants(ns + "protNFe").FirstOrDefault();
                if (protNFe != null)
                {
                    var infProt = protNFe.Element(ns + "infProt");
                    var nProt = infProt?.Element(ns + "nProt")?.Value;
                    var cStat = infProt?.Element(ns + "cStat")?.Value ?? "999";
                    var xMotivo = infProt?.Element(ns + "xMotivo")?.Value ?? string.Empty;
                    
                    // Se autorizado, montar XML protocolado
                    if (cStat == "100")
                    {
                        // TODO: Montar XML completo com protocolo
                        return (nProt, cStat, xMotivo, xmlRetorno);
                    }
                    
                    return (nProt, cStat, xMotivo, null);
                }

                return (null, "999", "Retorno inválido da SEFAZ", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar retorno da autorização");
                return (null, "999", ex.Message, null);
            }
        }

        private (string status, string motivo, string? protocolo) ProcessarRetornoInutilizacao(string xmlRetorno)
        {
            try
            {
                var doc = XDocument.Parse(xmlRetorno);
                var root = doc.Root;
                if (root == null)
                {
                    return ("999", "Retorno inválido da SEFAZ", null);
                }
                var ns = root.GetDefaultNamespace();

                var infInut = doc.Descendants(ns + "infInut").FirstOrDefault();
                if (infInut != null)
                {
                    var cStat = infInut.Element(ns + "cStat")?.Value ?? "999";
                    var xMotivo = infInut.Element(ns + "xMotivo")?.Value ?? string.Empty;
                    var nProt = infInut.Element(ns + "nProt")?.Value;
                    
                    return (cStat, xMotivo, nProt);
                }

                return ("999", "Retorno inválido da SEFAZ", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar retorno da inutilização");
                return ("999", ex.Message, null);
            }
        }

        private string ObterUFPorCodigo(int codigo)
        {
            var ufs = new Dictionary<int, string>
            {
                [12] = "AC", [27] = "AL", [16] = "AP", [13] = "AM", [29] = "BA",
                [23] = "CE", [53] = "DF", [32] = "ES", [52] = "GO", [21] = "MA",
                [51] = "MT", [50] = "MS", [31] = "MG", [15] = "PA", [25] = "PB",
                [41] = "PR", [26] = "PE", [22] = "PI", [33] = "RJ", [24] = "RN",
                [43] = "RS", [11] = "RO", [14] = "RR", [42] = "SC", [35] = "SP",
                [28] = "SE", [17] = "TO"
            };

            return ufs.ContainsKey(codigo) ? ufs[codigo] : "SP";
        }
    }

    public class NFeResult
    {
        public bool Sucesso { get; set; }
        public string Mensagem { get; set; } = string.Empty;
        public string Protocolo { get; set; } = string.Empty;
        public string ChaveAcesso { get; set; } = string.Empty;
        public int NumeroNFe { get; set; }
        public int Serie { get; set; }
        public DateTime? DataAutorizacao { get; set; }
        public string XmlAutorizado { get; set; } = string.Empty;
        public string CodigoStatus { get; set; } = string.Empty;
        public List<string> Erros { get; set; } = new List<string>();
    }

    public class EventoResult
    {
        public bool Sucesso { get; set; }
        public string Protocolo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataEvento { get; set; }
    }

    public class InutilizacaoResult
    {
        public bool Sucesso { get; set; }
        public string Protocolo { get; set; } = string.Empty;
        public string Mensagem { get; set; } = string.Empty;
        public DateTime DataInutilizacao { get; set; }
    }
}
