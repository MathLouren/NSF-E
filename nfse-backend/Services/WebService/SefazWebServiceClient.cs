using System;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Net.Security;
using nfse_backend.Services.Certificado;

namespace nfse_backend.Services.WebService
{
    public class SefazWebServiceClient
    {
        private readonly CertificadoDigitalService _certificadoService;
        private readonly HttpClient _httpClient;
        private Dictionary<string, Dictionary<string, string>> _urlsWebService = new();

        public SefazWebServiceClient(CertificadoDigitalService certificadoService)
        {
            _certificadoService = certificadoService;
            
            var handler = new HttpClientHandler();
            ConfigurarCertificado(handler);
            
            _httpClient = new HttpClient(handler);
            _httpClient.Timeout = TimeSpan.FromSeconds(100);
            
            InicializarUrlsWebService();
        }

        private void ConfigurarCertificado(HttpClientHandler handler)
        {
            try
            {
                var certificado = _certificadoService.ObterCertificado();
                if (certificado != null)
                {
                    handler.ClientCertificates.Add(certificado);
                }
            }
            catch (Exception)
            {
                // Certificado não carregado ainda - será configurado quando necessário
            }

            // Configurar validação SSL
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;
            
            // Configurar protocolos de segurança
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
        }

        private HttpClient ObterHttpClientAtualizado()
        {
            var handler = new HttpClientHandler();
            ConfigurarCertificado(handler);
            
            var client = new HttpClient(handler);
            client.Timeout = TimeSpan.FromSeconds(100);
            
            return client;
        }

        private void InicializarUrlsWebService()
        {
            _urlsWebService = new Dictionary<string, Dictionary<string, string>>();

            // 1) Tentativa de carregar de config/endpoints.json
            try
            {
                var endpointsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "endpoints.json");
                if (File.Exists(endpointsPath))
                {
                    var json = File.ReadAllText(endpointsPath);
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
                    if (dict != null)
                    {
                        // Mapear entradas conhecidas para chaves internas
                        foreach (var kv in dict)
                        {
                            if (kv.Key == "BR") continue;
                            var uf = kv.Key.ToUpper();
                            if (kv.Value.ContainsKey("homolog"))
                            {
                                _urlsWebService[$"{uf}_HOMOLOGACAO"] = new Dictionary<string, string>(kv.Value);
                            }
                            if (kv.Value.ContainsKey("prod"))
                            {
                                _urlsWebService[$"{uf}_PRODUCAO"] = new Dictionary<string, string>(kv.Value);
                            }
                        }
                    }
                }
            }
            catch { }

            // URLs para SP - Ambiente de Homologação
            _urlsWebService["SP_HOMOLOGACAO"] = new Dictionary<string, string>
            {
                ["NfeAutorizacao"] = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx",
                ["NfeRetAutorizacao"] = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx",
                ["NfeConsultaProtocolo"] = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeconsultaprotocolo4.asmx",
                ["NfeStatusServico"] = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfestatusservico4.asmx",
                ["NfeInutilizacao"] = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeinutilizacao4.asmx",
                ["RecepcaoEvento"] = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nferecepcaoevento4.asmx"
            };

            // URLs para SP - Ambiente de Produção
            _urlsWebService["SP_PRODUCAO"] = new Dictionary<string, string>
            {
                ["NfeAutorizacao"] = "https://nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx",
                ["NfeRetAutorizacao"] = "https://nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx",
                ["NfeConsultaProtocolo"] = "https://nfe.fazenda.sp.gov.br/ws/nfeconsultaprotocolo4.asmx",
                ["NfeStatusServico"] = "https://nfe.fazenda.sp.gov.br/ws/nfestatusservico4.asmx",
                ["NfeInutilizacao"] = "https://nfe.fazenda.sp.gov.br/ws/nfeinutilizacao4.asmx",
                ["RecepcaoEvento"] = "https://nfe.fazenda.sp.gov.br/ws/nferecepcaoevento4.asmx"
            };

            // URLs para SVRS (Sefaz Virtual RS) - Homologação
            _urlsWebService["SVRS_HOMOLOGACAO"] = new Dictionary<string, string>
            {
                ["NfeAutorizacao"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx",
                ["NfeRetAutorizacao"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx",
                ["NfeConsultaProtocolo"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx",
                ["NfeStatusServico"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx",
                ["NfeInutilizacao"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx",
                ["RecepcaoEvento"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx"
            };

            // URLs para RJ (utiliza SVRS) - Homologação
            _urlsWebService["RJ_HOMOLOGACAO"] = new Dictionary<string, string>
            {
                ["NfeAutorizacao"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx",
                ["NfeRetAutorizacao"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx",
                ["NfeConsultaProtocolo"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx",
                ["NfeStatusServico"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx",
                ["NfeInutilizacao"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx",
                ["RecepcaoEvento"] = "https://nfe-homologacao.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx"
            };

            // URLs para SVRS (Sefaz Virtual RS) - Produção
            _urlsWebService["SVRS_PRODUCAO"] = new Dictionary<string, string>
            {
                ["NfeAutorizacao"] = "https://nfe.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx",
                ["NfeRetAutorizacao"] = "https://nfe.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx",
                ["NfeConsultaProtocolo"] = "https://nfe.svrs.rs.gov.br/ws/NfeConsulta/NFeConsulta4.asmx",
                ["NfeStatusServico"] = "https://nfe.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx",
                ["NfeInutilizacao"] = "https://nfe.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx",
                ["RecepcaoEvento"] = "https://nfe.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx"
            };

            // URLs para RJ (utiliza SVRS) - Produção
            _urlsWebService["RJ_PRODUCAO"] = new Dictionary<string, string>
            {
                ["NfeAutorizacao"] = "https://nfe.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx",
                ["NfeRetAutorizacao"] = "https://nfe.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx",
                ["NfeConsultaProtocolo"] = "https://nfe.svrs.rs.gov.br/ws/NfeConsulta/NFeConsulta4.asmx",
                ["NfeStatusServico"] = "https://nfe.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx",
                ["NfeInutilizacao"] = "https://nfe.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx",
                ["RecepcaoEvento"] = "https://nfe.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx"
            };

            // Adicionar outras UFs conforme necessário...
        }

        public async Task<string> EnviarLoteNFe(string xmlLote, string uf, bool homologacao = true)
        {
            try
            {
                string ambiente = homologacao ? "HOMOLOGACAO" : "PRODUCAO";
                string chaveUrl = $"{uf}_{ambiente}";
                
                if (!_urlsWebService.ContainsKey(chaveUrl))
                {
                    chaveUrl = $"SVRS_{ambiente}"; // Fallback para SVRS
                }

                string url = _urlsWebService[chaveUrl]["NfeAutorizacao"];
                string soapAction = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4/nfeAutorizacaoLote";

                string soapEnvelope = CriarEnvelopeSoap(xmlLote, "nfeDadosMsg", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4");

                return await EnviarRequisicaoSoap(url, soapAction, soapEnvelope);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar lote NF-e: {ex.Message}", ex);
            }
        }

        public async Task<string> ConsultarRetornoAutorizacao(string numeroRecibo, string uf, bool homologacao = true)
        {
            try
            {
                string ambiente = homologacao ? "HOMOLOGACAO" : "PRODUCAO";
                string chaveUrl = $"{uf}_{ambiente}";
                
                if (!_urlsWebService.ContainsKey(chaveUrl))
                {
                    chaveUrl = $"SVRS_{ambiente}";
                }

                string url = _urlsWebService[chaveUrl]["NfeRetAutorizacao"];
                string soapAction = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRetAutorizacao4/nfeRetAutorizacaoLote";

                string xmlConsulta = $@"<?xml version='1.0' encoding='UTF-8'?>
                    <consReciNFe xmlns='http://www.portalfiscal.inf.br/nfe' versao='4.00'>
                        <tpAmb>{(homologacao ? "2" : "1")}</tpAmb>
                        <nRec>{numeroRecibo}</nRec>
                    </consReciNFe>";

                string soapEnvelope = CriarEnvelopeSoap(xmlConsulta, "nfeDadosMsg", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRetAutorizacao4");

                return await EnviarRequisicaoSoap(url, soapAction, soapEnvelope);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao consultar retorno de autorização: {ex.Message}", ex);
            }
        }

        public async Task<string> ConsultarProtocolo(string chaveAcesso, string uf, bool homologacao = true)
        {
            try
            {
                string ambiente = homologacao ? "HOMOLOGACAO" : "PRODUCAO";
                string chaveUrl = $"{uf}_{ambiente}";
                
                if (!_urlsWebService.ContainsKey(chaveUrl))
                {
                    chaveUrl = $"SVRS_{ambiente}";
                }

                string url = _urlsWebService[chaveUrl]["NfeConsultaProtocolo"];
                string soapAction = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeConsultaProtocolo4/nfeConsultaNF";

                string xmlConsulta = $@"<?xml version='1.0' encoding='UTF-8'?>
                    <consSitNFe xmlns='http://www.portalfiscal.inf.br/nfe' versao='4.00'>
                        <tpAmb>{(homologacao ? "2" : "1")}</tpAmb>
                        <xServ>CONSULTAR</xServ>
                        <chNFe>{chaveAcesso}</chNFe>
                    </consSitNFe>";

                string xmlAssinado = _certificadoService.AssinarXml(xmlConsulta, "consSitNFe");
                string soapEnvelope = CriarEnvelopeSoap(xmlAssinado, "nfeDadosMsg", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeConsultaProtocolo4");

                return await EnviarRequisicaoSoap(url, soapAction, soapEnvelope);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao consultar protocolo: {ex.Message}", ex);
            }
        }

        public async Task<string> ConsultarStatusServico(string uf, bool homologacao = true)
        {
            try
            {
                string ambiente = homologacao ? "HOMOLOGACAO" : "PRODUCAO";
                string chaveUrl = $"{uf}_{ambiente}";
                
                if (!_urlsWebService.ContainsKey(chaveUrl))
                {
                    chaveUrl = $"SVRS_{ambiente}";
                }

                string url = _urlsWebService[chaveUrl]["NfeStatusServico"];
                string soapAction = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4/nfeStatusServicoNF";

                string xmlConsulta = $@"<?xml version='1.0' encoding='UTF-8'?>
                    <consStatServ xmlns='http://www.portalfiscal.inf.br/nfe' versao='4.00'>
                        <tpAmb>{(homologacao ? "2" : "1")}</tpAmb>
                        <cUF>{ObterCodigoUF(uf)}</cUF>
                        <xServ>STATUS</xServ>
                    </consStatServ>";

                string xmlAssinado = _certificadoService.AssinarXml(xmlConsulta, "consStatServ");
                string soapEnvelope = CriarEnvelopeSoap(xmlAssinado, "nfeDadosMsg", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeStatusServico4");

                return await EnviarRequisicaoSoap(url, soapAction, soapEnvelope);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao consultar status do serviço: {ex.Message}", ex);
            }
        }

        public async Task<string> EnviarEvento(string xmlEvento, string uf, bool homologacao = true)
        {
            try
            {
                string ambiente = homologacao ? "HOMOLOGACAO" : "PRODUCAO";
                string chaveUrl = $"{uf}_{ambiente}";
                
                if (!_urlsWebService.ContainsKey(chaveUrl))
                {
                    chaveUrl = $"SVRS_{ambiente}";
                }

                string url = _urlsWebService[chaveUrl]["RecepcaoEvento"];
                string soapAction = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRecepcaoEvento4/nfeRecepcaoEvento";

                string soapEnvelope = CriarEnvelopeSoap(xmlEvento, "nfeDadosMsg", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeRecepcaoEvento4");

                return await EnviarRequisicaoSoap(url, soapAction, soapEnvelope);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar evento: {ex.Message}", ex);
            }
        }

        public async Task<string> InutilizarNumeracao(string xmlInutilizacao, string uf, bool homologacao = true)
        {
            try
            {
                string ambiente = homologacao ? "HOMOLOGACAO" : "PRODUCAO";
                string chaveUrl = $"{uf}_{ambiente}";
                
                if (!_urlsWebService.ContainsKey(chaveUrl))
                {
                    chaveUrl = $"SVRS_{ambiente}";
                }

                string url = _urlsWebService[chaveUrl]["NfeInutilizacao"];
                string soapAction = "http://www.portalfiscal.inf.br/nfe/wsdl/NFeInutilizacao4/nfeInutilizacaoNF";

                string soapEnvelope = CriarEnvelopeSoap(xmlInutilizacao, "nfeDadosMsg", "http://www.portalfiscal.inf.br/nfe/wsdl/NFeInutilizacao4");

                return await EnviarRequisicaoSoap(url, soapAction, soapEnvelope);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao inutilizar numeração: {ex.Message}", ex);
            }
        }

        private string CriarEnvelopeSoap(string xmlBody, string tagBody, string xmlns)
        {
            return $@"<?xml version='1.0' encoding='utf-8'?>
                <soap12:Envelope xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' 
                                xmlns:xsd='http://www.w3.org/2001/XMLSchema' 
                                xmlns:soap12='http://www.w3.org/2003/05/soap-envelope'>
                    <soap12:Body>
                        <{tagBody} xmlns='{xmlns}'>
                            {xmlBody}
                        </{tagBody}>
                    </soap12:Body>
                </soap12:Envelope>";
        }

        private async Task<string> EnviarRequisicaoSoap(string url, string soapAction, string soapEnvelope)
        {
            try
            {
                using var httpClient = ObterHttpClientAtualizado();
                var content = new StringContent(soapEnvelope, Encoding.UTF8, "application/soap+xml");
                
                if (!string.IsNullOrEmpty(soapAction))
                {
                    httpClient.DefaultRequestHeaders.Clear();
                    httpClient.DefaultRequestHeaders.Add("SOAPAction", soapAction);
                }

                var response = await httpClient.PostAsync(url, content);
                
                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Erro HTTP: {response.StatusCode} - {response.ReasonPhrase}");
                }

                var responseContent = await response.Content.ReadAsStringAsync();
                return ExtrairConteudoSoap(responseContent);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro na requisição SOAP: {ex.Message}", ex);
            }
        }

        private string ExtrairConteudoSoap(string soapResponse)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(soapResponse);

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
                nsmgr.AddNamespace("soap", "http://www.w3.org/2003/05/soap-envelope");
                nsmgr.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");

                // Procura pelo Body
                XmlNode? bodyNode = doc.SelectSingleNode("//soap:Body", nsmgr) ?? 
                                   doc.SelectSingleNode("//soap12:Body", nsmgr);

                if (bodyNode != null && bodyNode.HasChildNodes && bodyNode.FirstChild != null)
                {
                    // Retorna o conteúdo do primeiro filho do Body
                    return bodyNode.FirstChild.InnerXml;
                }

                return soapResponse;
            }
            catch
            {
                return soapResponse;
            }
        }

        private int ObterCodigoUF(string uf)
        {
            var codigosUF = new Dictionary<string, int>
            {
                ["AC"] = 12, ["AL"] = 27, ["AP"] = 16, ["AM"] = 13, ["BA"] = 29,
                ["CE"] = 23, ["DF"] = 53, ["ES"] = 32, ["GO"] = 52, ["MA"] = 21,
                ["MT"] = 51, ["MS"] = 50, ["MG"] = 31, ["PA"] = 15, ["PB"] = 25,
                ["PR"] = 41, ["PE"] = 26, ["PI"] = 22, ["RJ"] = 33, ["RN"] = 24,
                ["RS"] = 43, ["RO"] = 11, ["RR"] = 14, ["SC"] = 42, ["SP"] = 35,
                ["SE"] = 28, ["TO"] = 17
            };

            return codigosUF.ContainsKey(uf) ? codigosUF[uf] : 0;
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }
    }
}
