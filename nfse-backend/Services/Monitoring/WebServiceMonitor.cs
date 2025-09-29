using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using nfse_backend.Services.Configuracao;

namespace nfse_backend.Services.Monitoring
{
    /// <summary>
    /// Monitor de WebServices da SEFAZ baseado na documentação oficial
    /// Verifica disponibilidade e status dos serviços conforme Portal Nacional da NF-e
    /// </summary>
    public class WebServiceMonitor
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<WebServiceMonitor> _logger;
        private readonly ConfiguracaoNFeService _configuracaoService;
        private readonly Dictionary<string, DateTime> _ultimaVerificacao;
        private readonly Dictionary<string, bool> _statusServicos;

        public WebServiceMonitor(
            HttpClient httpClient,
            ILogger<WebServiceMonitor> logger,
            ConfiguracaoNFeService configuracaoService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _configuracaoService = configuracaoService;
            _ultimaVerificacao = new Dictionary<string, DateTime>();
            _statusServicos = new Dictionary<string, bool>();
            
            ConfigurarHttpClient();
        }

        private void ConfigurarHttpClient()
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(30);
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NFe-Monitor/1.0");
        }

        /// <summary>
        /// Verifica disponibilidade de um WebService específico
        /// Baseado no padrão da documentação oficial: adicionar ?wsdl ao final da URL
        /// </summary>
        public async Task<WebServiceStatus> VerificarDisponibilidadeAsync(string url, string nomeServico)
        {
            var chaveCache = $"{url}_{nomeServico}";
            
            try
            {
                _logger.LogDebug($"Verificando disponibilidade do WebService: {nomeServico} - {url}");

                // Adicionar ?wsdl conforme documentação oficial
                var urlWsdl = url.EndsWith("?wsdl") ? url : $"{url}?wsdl";
                
                var startTime = DateTime.Now;
                var response = await _httpClient.GetAsync(urlWsdl);
                var responseTime = DateTime.Now - startTime;

                var status = new WebServiceStatus
                {
                    NomeServico = nomeServico,
                    Url = url,
                    Disponivel = response.IsSuccessStatusCode,
                    TempoResposta = responseTime,
                    UltimaVerificacao = DateTime.Now,
                    CodigoHttp = (int)response.StatusCode,
                    MensagemStatus = response.ReasonPhrase ?? "OK"
                };

                // Validar se o WSDL é válido
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    status.WsdlValido = ValidarWsdl(content);
                    
                    if (!status.WsdlValido)
                    {
                        status.Disponivel = false;
                        status.MensagemStatus = "WSDL inválido ou não encontrado";
                    }
                }

                // Atualizar cache
                _ultimaVerificacao[chaveCache] = DateTime.Now;
                _statusServicos[chaveCache] = status.Disponivel;

                _logger.LogInformation($"WebService {nomeServico}: {(status.Disponivel ? "DISPONÍVEL" : "INDISPONÍVEL")} " +
                                     $"- Tempo: {responseTime.TotalMilliseconds}ms");

                return status;
            }
            catch (TaskCanceledException ex) when (ex.InnerException is TimeoutException)
            {
                _logger.LogWarning($"Timeout ao verificar WebService {nomeServico}: {url}");
                return CriarStatusIndisponivel(nomeServico, url, "Timeout", 408);
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning($"Erro HTTP ao verificar WebService {nomeServico}: {ex.Message}");
                return CriarStatusIndisponivel(nomeServico, url, ex.Message, 0);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado ao verificar WebService {nomeServico}");
                return CriarStatusIndisponivel(nomeServico, url, ex.Message, 0);
            }
        }

        /// <summary>
        /// Verifica disponibilidade de todos os WebServices de uma UF
        /// </summary>
        public async Task<List<WebServiceStatus>> VerificarDisponibilidadeUFAsync(string uf, bool homologacao = true)
        {
            var resultados = new List<WebServiceStatus>();
            
            try
            {
                var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
                var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);

                var servicos = new Dictionary<string, string>
                {
                    ["NFeAutorizacao4"] = urls.UrlNfeAutorizacao,
                    ["NFeRetAutorizacao4"] = urls.UrlNfeRetAutorizacao,
                    ["NFeConsultaProtocolo4"] = urls.UrlNfeConsultaProtocolo,
                    ["NFeStatusServico4"] = urls.UrlNfeStatusServico,
                    ["NFeInutilizacao4"] = urls.UrlNfeInutilizacao,
                    ["RecepcaoEvento4"] = urls.UrlRecepcaoEvento,
                    ["NFeDistribuicaoDFe"] = urls.UrlNfeDistribuicaoDFe
                };

                var tasks = new List<Task<WebServiceStatus>>();
                
                foreach (var servico in servicos)
                {
                    if (!string.IsNullOrEmpty(servico.Value))
                    {
                        tasks.Add(VerificarDisponibilidadeAsync(servico.Value, servico.Key));
                    }
                }

                var resultadosArray = await Task.WhenAll(tasks);
                resultados.AddRange(resultadosArray);

                var disponiveis = resultados.Count(r => r.Disponivel);
                var total = resultados.Count;
                
                _logger.LogInformation($"Status WebServices {uf} ({(homologacao ? "Homologação" : "Produção")}): " +
                                     $"{disponiveis}/{total} disponíveis");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao verificar disponibilidade dos WebServices da UF {uf}");
            }

            return resultados;
        }

        /// <summary>
        /// Verifica status do serviço através do próprio WebService (NFeStatusServico)
        /// </summary>
        public async Task<StatusServicoSefaz> VerificarStatusServicoSefazAsync(string uf, bool homologacao = true)
        {
            try
            {
                var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
                var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
                var urlStatus = urls.UrlNfeStatusServico;

                if (string.IsNullOrEmpty(urlStatus))
                {
                    return new StatusServicoSefaz
                    {
                        UF = uf,
                        Disponivel = false,
                        MensagemStatus = "URL do serviço de status não configurada"
                    };
                }

                // Criar XML de consulta de status
                var xmlConsulta = CriarXmlConsultaStatus(uf, homologacao);
                var soapEnvelope = CriarSoapEnvelopeStatus(xmlConsulta);

                var content = new StringContent(soapEnvelope, System.Text.Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4/nfeStatusServicoNF");

                var response = await _httpClient.PostAsync(urlStatus, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                return ProcessarRespostaStatus(responseContent, uf);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao verificar status do serviço SEFAZ {uf}");
                return new StatusServicoSefaz
                {
                    UF = uf,
                    Disponivel = false,
                    MensagemStatus = ex.Message
                };
            }
        }

        /// <summary>
        /// Valida se o conteúdo é um WSDL válido
        /// </summary>
        private bool ValidarWsdl(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                    return false;

                // Verificar se contém elementos básicos de um WSDL
                return content.Contains("<wsdl:definitions") || 
                       content.Contains("<definitions") ||
                       (content.Contains("<types>") && content.Contains("<service>"));
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Cria status indisponível
        /// </summary>
        private WebServiceStatus CriarStatusIndisponivel(string nomeServico, string url, string mensagem, int codigoHttp)
        {
            return new WebServiceStatus
            {
                NomeServico = nomeServico,
                Url = url,
                Disponivel = false,
                TempoResposta = TimeSpan.Zero,
                UltimaVerificacao = DateTime.Now,
                CodigoHttp = codigoHttp,
                MensagemStatus = mensagem,
                WsdlValido = false
            };
        }

        /// <summary>
        /// Cria XML para consulta de status do serviço
        /// </summary>
        private string CriarXmlConsultaStatus(string uf, bool homologacao)
        {
            var ambiente = homologacao ? 2 : 1;
            var cUF = ObterCodigoUF(uf);

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<consStatServ xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""4.00"">
    <tpAmb>{ambiente}</tpAmb>
    <cUF>{cUF}</cUF>
    <xServ>STATUS</xServ>
</consStatServ>";
        }

        /// <summary>
        /// Cria envelope SOAP para consulta de status
        /// </summary>
        private string CriarSoapEnvelopeStatus(string xmlConsulta)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlConsulta}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

        /// <summary>
        /// Processa resposta do status do serviço
        /// </summary>
        private StatusServicoSefaz ProcessarRespostaStatus(string responseContent, string uf)
        {
            try
            {
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(responseContent);

                var nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                nsmgr.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");

                var cStat = xmlDoc.SelectSingleNode("//nfe:cStat", nsmgr)?.InnerText;
                var xMotivo = xmlDoc.SelectSingleNode("//nfe:xMotivo", nsmgr)?.InnerText;
                var dhResp = xmlDoc.SelectSingleNode("//nfe:dhResp", nsmgr)?.InnerText;
                var verAplic = xmlDoc.SelectSingleNode("//nfe:verAplic", nsmgr)?.InnerText;

                var disponivel = cStat == "107"; // Código 107 = Serviço em Operação

                return new StatusServicoSefaz
                {
                    UF = uf,
                    Disponivel = disponivel,
                    CodigoStatus = cStat,
                    MensagemStatus = xMotivo ?? "Status não informado",
                    DataHoraResposta = DateTime.TryParse(dhResp, out var data) ? data : DateTime.Now,
                    VersaoAplicacao = verAplic
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao processar resposta de status da SEFAZ {uf}");
                return new StatusServicoSefaz
                {
                    UF = uf,
                    Disponivel = false,
                    MensagemStatus = $"Erro ao processar resposta: {ex.Message}"
                };
            }
        }

        /// <summary>
        /// Obtém código da UF
        /// </summary>
        private int ObterCodigoUF(string uf)
        {
            var codigosUF = new Dictionary<string, int>
            {
                ["AC"] = 12, ["AL"] = 17, ["AP"] = 16, ["AM"] = 23, ["BA"] = 29,
                ["CE"] = 23, ["DF"] = 53, ["ES"] = 32, ["GO"] = 52, ["MA"] = 21,
                ["MT"] = 51, ["MS"] = 50, ["MG"] = 31, ["PA"] = 15, ["PB"] = 25,
                ["PR"] = 41, ["PE"] = 26, ["PI"] = 22, ["RJ"] = 33, ["RN"] = 24,
                ["RS"] = 43, ["RO"] = 11, ["RR"] = 14, ["SC"] = 42, ["SP"] = 35,
                ["SE"] = 28, ["TO"] = 17
            };

            return codigosUF.ContainsKey(uf.ToUpper()) ? codigosUF[uf.ToUpper()] : 33;
        }
    }

    /// <summary>
    /// Status de um WebService
    /// </summary>
    public class WebServiceStatus
    {
        public string NomeServico { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool Disponivel { get; set; }
        public TimeSpan TempoResposta { get; set; }
        public DateTime UltimaVerificacao { get; set; }
        public int CodigoHttp { get; set; }
        public string MensagemStatus { get; set; } = string.Empty;
        public bool WsdlValido { get; set; }
    }

    /// <summary>
    /// Status do serviço SEFAZ (através do próprio WebService)
    /// </summary>
    public class StatusServicoSefaz
    {
        public string UF { get; set; } = string.Empty;
        public bool Disponivel { get; set; }
        public string? CodigoStatus { get; set; }
        public string MensagemStatus { get; set; } = string.Empty;
        public DateTime DataHoraResposta { get; set; }
        public string? VersaoAplicacao { get; set; }
    }
}
