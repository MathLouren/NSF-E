using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Logging;
using nfse_backend.Services.Certificado;
using nfse_backend.Services.Configuracao;
using System.Net.Security;
using System.Security.Authentication;
using Polly;
using Polly.Extensions.Http;

namespace nfse_backend.Services.WebService
{
    public class SefazWebServiceClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<SefazWebServiceClient> _logger;
        private readonly CertificadoDigitalService _certificadoService;
        private readonly ConfiguracaoNFeService _configuracaoService;

        public SefazWebServiceClient(
            HttpClient httpClient,
            ILogger<SefazWebServiceClient> logger,
            CertificadoDigitalService certificadoService,
            ConfiguracaoNFeService configuracaoService)
        {
            _httpClient = httpClient;
            _logger = logger;
            _certificadoService = certificadoService;
            _configuracaoService = configuracaoService;
            
            ConfigurarHttpClient();
        }

        private void ConfigurarHttpClient()
        {
            _httpClient.Timeout = TimeSpan.FromMinutes(5); // Timeout de 5 minutos
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "NFe-Emissor/1.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "text/xml, application/soap+xml");
        }

        public async Task<string> EnviarLoteNFe(string xmlLote, string uf, bool homologacao = true)
        {
            var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
            var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
            var urlAutorizacao = urls.UrlNfeAutorizacao;

            return await ExecutarComRetry(async () =>
            {
                var soapEnvelope = CriarSoapEnvelopeAutorizacao(xmlLote);
                return await EnviarSoap(urlAutorizacao, soapEnvelope, "http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4/nfeAutorizacaoLote");
            }, "EnviarLoteNFe");
        }

        public async Task<string> ConsultarRecibo(string numeroRecibo, string uf, bool homologacao = true)
        {
            var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
            var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
            var urlRetAutorizacao = urls.UrlNfeRetAutorizacao;

            return await ExecutarComRetry(async () =>
            {
                var soapEnvelope = CriarSoapEnvelopeConsultaRecibo(numeroRecibo);
                return await EnviarSoap(urlRetAutorizacao, soapEnvelope, "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRetAutorizacao4/nfeRetAutorizacaoLote");
            }, "ConsultarRecibo");
        }

        public async Task<string> ConsultarProtocolo(string chaveAcesso, string uf, bool homologacao = true)
        {
            var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
            var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
            var urlConsulta = urls.UrlNfeConsultaProtocolo;

            return await ExecutarComRetry(async () =>
            {
                var soapEnvelope = CriarSoapEnvelopeConsultaProtocolo(chaveAcesso);
                return await EnviarSoap(urlConsulta, soapEnvelope, "http://www.portalfiscal.inf.br/nfe/wsdl/NFeConsultaProtocolo4/nfeConsultaNF");
            }, "ConsultarProtocolo");
        }

        public async Task<string> EnviarEvento(string xmlEvento, string uf, bool homologacao = true)
        {
            var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
            var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
            var urlEvento = urls.UrlRecepcaoEvento;

            return await ExecutarComRetry(async () =>
            {
                var soapEnvelope = CriarSoapEnvelopeEvento(xmlEvento);
                return await EnviarSoap(urlEvento, soapEnvelope, "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRecepcaoEvento4/nfeRecepcaoEvento");
            }, "EnviarEvento");
        }

        public async Task<string> InutilizarNumeracao(string xmlInutilizacao, string uf, bool homologacao = true)
        {
            var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
            var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
            var urlInutilizacao = urls.UrlNfeInutilizacao;

            return await ExecutarComRetry(async () =>
            {
                var soapEnvelope = CriarSoapEnvelopeInutilizacao(xmlInutilizacao);
                return await EnviarSoap(urlInutilizacao, soapEnvelope, "http://www.portalfiscal.inf.br/nfe/wsdl/NFeInutilizacao4/nfeInutilizacaoNF");
            }, "InutilizarNumeracao");
        }

        public async Task<string> ConsultarStatusServico(string uf, bool homologacao = true)
        {
            var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
            var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
            var urlStatus = urls.UrlNfeStatusServico;

            return await ExecutarComRetry(async () =>
            {
                var soapEnvelope = CriarSoapEnvelopeStatusServico(uf, homologacao);
                return await EnviarSoap(urlStatus, soapEnvelope, "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4/nfeStatusServicoNF");
            }, "ConsultarStatusServico");
        }

        private async Task<T> ExecutarComRetry<T>(Func<Task<T>> operacao, string nomeOperacao)
        {
            // Política de retry mais robusta com jitter e fallback
            var retryPolicy = Policy
                .Handle<HttpRequestException>()
                .Or<TaskCanceledException>()
                .Or<AuthenticationException>()
                .Or<System.Net.Sockets.SocketException>()
                .Or<System.IO.IOException>()
                .WaitAndRetryAsync(
                    retryCount: 5,
                    sleepDurationProvider: retryAttempt => 
                    {
                        // Backoff exponencial com jitter para evitar thundering herd
                        var baseDelay = TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                        var jitter = TimeSpan.FromMilliseconds(Random.Shared.Next(0, 1000));
                        return baseDelay.Add(jitter);
                    },
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var errorMessage = outcome?.ToString() ?? "Erro desconhecido";
                        _logger.LogWarning($"Tentativa {retryCount}/5 de {nomeOperacao} falhou. Tentando novamente em {timespan.TotalSeconds:F1}s. Erro: {errorMessage}");
                        
                        // Log adicional para análise - usando pattern matching seguro
                        if (outcome is System.Exception ex && ex is HttpRequestException httpEx)
                        {
                            _logger.LogWarning($"Detalhes HTTP: {httpEx.Data}");
                        }
                    });

            try
            {
                return await retryPolicy.ExecuteAsync(operacao);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Falha definitiva em {nomeOperacao} após 5 tentativas. Última exceção: {ex.GetType().Name}");
                
                // Tentar fallback para contingência se aplicável
                if (nomeOperacao.Contains("EnviarLote") || nomeOperacao.Contains("ConsultarRecibo"))
                {
                    _logger.LogWarning($"Considerando ativação de contingência devido à falha em {nomeOperacao}");
                }
                
                throw new SefazIndisponivelException($"SEFAZ indisponível após múltiplas tentativas: {ex.Message}", ex);
            }
        }

        private async Task<string> EnviarSoap(string url, string soapEnvelope, string soapAction)
        {
            try
            {
                _logger.LogDebug($"Enviando SOAP para: {url}");
                _logger.LogTrace($"SOAP Envelope: {soapEnvelope}");

                // Configurar certificado se necessário
                var certificado = _certificadoService.ObterCertificado();
                if (certificado != null)
                {
                    var handler = new HttpClientHandler();
                    handler.ClientCertificates.Add(certificado);
                    handler.SslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13;
                    
                    using var clientComCertificado = new HttpClient(handler);
                    clientComCertificado.Timeout = _httpClient.Timeout;
                    return await EnviarSoapInterno(clientComCertificado, url, soapEnvelope, soapAction);
                }

                return await EnviarSoapInterno(_httpClient, url, soapEnvelope, soapAction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao enviar SOAP para {url}");
                throw;
            }
        }

        private async Task<string> EnviarSoapInterno(HttpClient client, string url, string soapEnvelope, string soapAction)
        {
            var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", soapAction);

            var response = await client.PostAsync(url, content);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Erro HTTP {response.StatusCode}: {errorContent}");
                throw new HttpRequestException($"Erro HTTP {response.StatusCode}: {response.ReasonPhrase}");
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogTrace($"Resposta SOAP: {responseContent}");
            
            return responseContent;
        }

        private string CriarSoapEnvelopeAutorizacao(string xmlLote)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlLote}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

        private string CriarSoapEnvelopeConsultaRecibo(string numeroRecibo)
        {
            var ambiente = _configuracaoService.ObterAmbienteAtual();
            var xmlConsulta = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<consReciNFe xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""4.00"">
    <tpAmb>{(int)ambiente}</tpAmb>
    <nRec>{numeroRecibo}</nRec>
</consReciNFe>";

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeRetAutorizacao4"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlConsulta}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

        private string CriarSoapEnvelopeConsultaProtocolo(string chaveAcesso)
        {
            var ambiente = _configuracaoService.ObterAmbienteAtual();
            var xmlConsulta = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<consSitNFe xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""4.00"">
    <tpAmb>{(int)ambiente}</tpAmb>
    <xServ>CONSULTAR</xServ>
    <chNFe>{chaveAcesso}</chNFe>
</consSitNFe>";

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeConsultaProtocolo4"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlConsulta}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

        private string CriarSoapEnvelopeEvento(string xmlEvento)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeRecepcaoEvento4"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlEvento}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

        private string CriarSoapEnvelopeInutilizacao(string xmlInutilizacao)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeInutilizacao4"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlInutilizacao}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

        private string CriarSoapEnvelopeStatusServico(string uf, bool homologacao)
        {
            var ambiente = homologacao ? 2 : 1;
            var cUF = ObterCodigoUF(uf);
            
            var xmlStatus = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<consStatServ xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""4.00"">
    <tpAmb>{ambiente}</tpAmb>
    <cUF>{cUF}</cUF>
    <xServ>STATUS</xServ>
</consStatServ>";

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlStatus}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

        public async Task<string> EnviarEventoNFe(string xmlEvento, string uf, bool homologacao = true)
        {
            return await EnviarEvento(xmlEvento, uf, homologacao);
        }

        public async Task<string> ConsultarProtocoloNFe(string chaveAcesso, string uf, bool homologacao = true)
        {
            return await ConsultarProtocolo(chaveAcesso, uf, homologacao);
        }

        public async Task<string> ConsultarDistribuicaoDFe(string cnpj, string uf, string ultimoNSU = "0", bool homologacao = true)
        {
            var ambiente = homologacao ? AmbienteNFe.Homologacao : AmbienteNFe.Producao;
            var urls = _configuracaoService.ObterUrlsWebService(uf, ambiente);
            var urlDistribuicao = urls.UrlNfeDistribuicaoDFe;

            return await ExecutarComRetry(async () =>
            {
                var soapEnvelope = CriarSoapEnvelopeDistribuicaoDFe(cnpj, ultimoNSU, uf, homologacao);
                return await EnviarSoap(urlDistribuicao, soapEnvelope, "http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe/nfeDistDFeInteresse");
            }, "ConsultarDistribuicaoDFe");
        }

        private string CriarSoapEnvelopeDistribuicaoDFe(string cnpj, string ultimoNSU, string uf, bool homologacao)
        {
            var ambiente = homologacao ? 2 : 1;
            var cUF = ObterCodigoUF(uf);
            
            var xmlDistribuicao = $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<distDFeInt xmlns=""http://www.portalfiscal.inf.br/nfe"" versao=""1.01"">
    <tpAmb>{ambiente}</tpAmb>
    <cUFAutor>{cUF}</cUFAutor>
    <CNPJ>{cnpj}</CNPJ>
    <distNSU>
        <ultNSU>{ultimoNSU}</ultNSU>
    </distNSU>
</distDFeInt>";

            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:nfe=""http://www.portalfiscal.inf.br/nfe/wsdl/NFeDistribuicaoDFe"">
    <soap:Header/>
    <soap:Body>
        <nfe:nfeDadosMsg>
            <![CDATA[{xmlDistribuicao}]]>
        </nfe:nfeDadosMsg>
    </soap:Body>
</soap:Envelope>";
        }

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
}