using System;
using System.Xml.Linq;
using System.Threading.Tasks;
using nfse_backend.Services.Certificado;
using nfse_backend.Services.WebService;
using nfse_backend.Services.Xml;

namespace nfse_backend.Services.Eventos
{
    public class EventosNFeService
    {
        private readonly CertificadoDigitalService _certificadoService;
        private readonly SefazWebServiceClient _webServiceClient;
        private readonly XmlValidationService _validationService;
        private readonly XNamespace nfe = "http://www.portalfiscal.inf.br/nfe";

        public EventosNFeService(
            CertificadoDigitalService certificadoService,
            SefazWebServiceClient webServiceClient,
            XmlValidationService validationService)
        {
            _certificadoService = certificadoService;
            _webServiceClient = webServiceClient;
            _validationService = validationService;
        }

        public async Task<string> EnviarCartaCorrecao(
            string chaveAcesso,
            string cnpjEmitente,
            string textoCorrecao,
            int sequenciaEvento,
            string uf,
            bool homologacao = true)
        {
            try
            {
                // Validar parâmetros
                if (string.IsNullOrEmpty(chaveAcesso) || chaveAcesso.Length != 44)
                {
                    throw new ArgumentException("Chave de acesso inválida");
                }

                if (string.IsNullOrEmpty(textoCorrecao) || textoCorrecao.Length < 15 || textoCorrecao.Length > 1000)
                {
                    throw new ArgumentException("Texto da correção deve ter entre 15 e 1000 caracteres");
                }

                // Gerar XML do evento
                string xmlEvento = GerarXmlCartaCorrecao(
                    chaveAcesso,
                    cnpjEmitente,
                    textoCorrecao,
                    sequenciaEvento,
                    homologacao);

                // Assinar o XML
                string xmlAssinado = _certificadoService.AssinarXml(xmlEvento, "infEvento");

                // Criar envelope do evento
                string xmlEnvelope = CriarEnvelopeEvento(xmlAssinado, homologacao);

                // Enviar para SEFAZ
                string retorno = await _webServiceClient.EnviarEvento(xmlEnvelope, uf, homologacao);

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao enviar carta de correção: {ex.Message}", ex);
            }
        }

        public async Task<string> CancelarNFe(
            string chaveAcesso,
            string protocolo,
            string cnpjEmitente,
            string justificativa,
            string uf,
            bool homologacao = true)
        {
            try
            {
                // Validar parâmetros
                if (string.IsNullOrEmpty(chaveAcesso) || chaveAcesso.Length != 44)
                {
                    throw new ArgumentException("Chave de acesso inválida");
                }

                if (string.IsNullOrEmpty(protocolo))
                {
                    throw new ArgumentException("Protocolo de autorização é obrigatório");
                }

                if (string.IsNullOrEmpty(justificativa) || justificativa.Length < 15 || justificativa.Length > 255)
                {
                    throw new ArgumentException("Justificativa deve ter entre 15 e 255 caracteres");
                }

                // Gerar XML do evento de cancelamento
                string xmlEvento = GerarXmlCancelamento(
                    chaveAcesso,
                    protocolo,
                    cnpjEmitente,
                    justificativa,
                    homologacao);

                // Assinar o XML
                string xmlAssinado = _certificadoService.AssinarXml(xmlEvento, "infEvento");

                // Criar envelope do evento
                string xmlEnvelope = CriarEnvelopeEvento(xmlAssinado, homologacao);

                // Enviar para SEFAZ
                string retorno = await _webServiceClient.EnviarEvento(xmlEnvelope, uf, homologacao);

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao cancelar NF-e: {ex.Message}", ex);
            }
        }

        public async Task<string> InutilizarNumeracao(
            string cnpjEmitente,
            string uf,
            int ano,
            int serie,
            int numeroInicial,
            int numeroFinal,
            string justificativa,
            bool homologacao = true)
        {
            try
            {
                // Validar parâmetros
                if (string.IsNullOrEmpty(cnpjEmitente) || cnpjEmitente.Length != 14)
                {
                    throw new ArgumentException("CNPJ inválido");
                }

                if (string.IsNullOrEmpty(justificativa) || justificativa.Length < 15 || justificativa.Length > 255)
                {
                    throw new ArgumentException("Justificativa deve ter entre 15 e 255 caracteres");
                }

                if (numeroInicial > numeroFinal)
                {
                    throw new ArgumentException("Número inicial não pode ser maior que o número final");
                }

                // Gerar XML de inutilização
                string xmlInutilizacao = GerarXmlInutilizacao(
                    cnpjEmitente,
                    uf,
                    ano,
                    serie,
                    numeroInicial,
                    numeroFinal,
                    justificativa,
                    homologacao);

                // Assinar o XML
                string xmlAssinado = _certificadoService.AssinarXml(xmlInutilizacao, "infInut");

                // Criar envelope
                string xmlEnvelope = CriarEnvelopeInutilizacao(xmlAssinado);

                // Enviar para SEFAZ
                string retorno = await _webServiceClient.InutilizarNumeracao(xmlEnvelope, uf, homologacao);

                return retorno;
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao inutilizar numeração: {ex.Message}", ex);
            }
        }

        private string GerarXmlCartaCorrecao(
            string chaveAcesso,
            string cnpjEmitente,
            string textoCorrecao,
            int sequenciaEvento,
            bool homologacao)
        {
            var cOrgao = int.Parse(chaveAcesso.Substring(0, 2));
            var dhEvento = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            var tpEvento = "110110"; // Código do evento de Carta de Correção
            var idEvento = $"ID{tpEvento}{chaveAcesso}{sequenciaEvento:00}";

            var xml = new XDocument(
                new XElement(nfe + "evento",
                    new XAttribute("versao", "1.00"),
                    new XElement(nfe + "infEvento",
                        new XAttribute("Id", idEvento),
                        new XElement(nfe + "cOrgao", cOrgao),
                        new XElement(nfe + "tpAmb", homologacao ? "2" : "1"),
                        new XElement(nfe + "CNPJ", cnpjEmitente),
                        new XElement(nfe + "chNFe", chaveAcesso),
                        new XElement(nfe + "dhEvento", dhEvento),
                        new XElement(nfe + "tpEvento", tpEvento),
                        new XElement(nfe + "nSeqEvento", sequenciaEvento),
                        new XElement(nfe + "verEvento", "1.00"),
                        new XElement(nfe + "detEvento",
                            new XAttribute("versao", "1.00"),
                            new XElement(nfe + "descEvento", "Carta de Correcao"),
                            new XElement(nfe + "xCorrecao", textoCorrecao),
                            new XElement(nfe + "xCondUso",
                                "A Carta de Correcao e disciplinada pelo paragrafo 1o-A do art. 7o do Convenio S/N, " +
                                "de 15 de dezembro de 1970 e pode ser utilizada para regularizacao de erro ocorrido na " +
                                "emissao de documento fiscal, desde que o erro nao esteja relacionado com: I - as variaveis " +
                                "que determinam o valor do imposto tais como: base de calculo, aliquota, diferenca de preco, " +
                                "quantidade, valor da operacao ou da prestacao; II - a correcao de dados cadastrais que " +
                                "implique mudanca do remetente ou do destinatario; III - a data de emissao ou de saida.")
                        )
                    )
                )
            );

            return xml.ToString();
        }

        private string GerarXmlCancelamento(
            string chaveAcesso,
            string protocolo,
            string cnpjEmitente,
            string justificativa,
            bool homologacao)
        {
            var cOrgao = int.Parse(chaveAcesso.Substring(0, 2));
            var dhEvento = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            var tpEvento = "110111"; // Código do evento de Cancelamento
            var idEvento = $"ID{tpEvento}{chaveAcesso}01";

            var xml = new XDocument(
                new XElement(nfe + "evento",
                    new XAttribute("versao", "1.00"),
                    new XElement(nfe + "infEvento",
                        new XAttribute("Id", idEvento),
                        new XElement(nfe + "cOrgao", cOrgao),
                        new XElement(nfe + "tpAmb", homologacao ? "2" : "1"),
                        new XElement(nfe + "CNPJ", cnpjEmitente),
                        new XElement(nfe + "chNFe", chaveAcesso),
                        new XElement(nfe + "dhEvento", dhEvento),
                        new XElement(nfe + "tpEvento", tpEvento),
                        new XElement(nfe + "nSeqEvento", "1"),
                        new XElement(nfe + "verEvento", "1.00"),
                        new XElement(nfe + "detEvento",
                            new XAttribute("versao", "1.00"),
                            new XElement(nfe + "descEvento", "Cancelamento"),
                            new XElement(nfe + "nProt", protocolo),
                            new XElement(nfe + "xJust", justificativa)
                        )
                    )
                )
            );

            return xml.ToString();
        }

        private string GerarXmlInutilizacao(
            string cnpjEmitente,
            string uf,
            int ano,
            int serie,
            int numeroInicial,
            int numeroFinal,
            string justificativa,
            bool homologacao)
        {
            var cUF = ObterCodigoUF(uf);
            var mod = "55"; // Modelo NF-e
            var dhEvento = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:sszzz");
            
            // ID = "ID" + cUF + ano + CNPJ + mod + serie + nNFIni + nNFFin
            var idInut = $"ID{cUF:00}{ano:00}{cnpjEmitente}{mod}{serie:000}{numeroInicial:000000000}{numeroFinal:000000000}";

            var xml = new XDocument(
                new XElement(nfe + "inutNFe",
                    new XAttribute("versao", "4.00"),
                    new XElement(nfe + "infInut",
                        new XAttribute("Id", idInut),
                        new XElement(nfe + "tpAmb", homologacao ? "2" : "1"),
                        new XElement(nfe + "xServ", "INUTILIZAR"),
                        new XElement(nfe + "cUF", cUF),
                        new XElement(nfe + "ano", ano.ToString().Substring(2, 2)), // Últimos 2 dígitos do ano
                        new XElement(nfe + "CNPJ", cnpjEmitente),
                        new XElement(nfe + "mod", mod),
                        new XElement(nfe + "serie", serie),
                        new XElement(nfe + "nNFIni", numeroInicial),
                        new XElement(nfe + "nNFFin", numeroFinal),
                        new XElement(nfe + "xJust", justificativa)
                    )
                )
            );

            return xml.ToString();
        }

        private string CriarEnvelopeEvento(string xmlEvento, bool homologacao)
        {
            var xml = new XDocument(
                new XElement(nfe + "envEvento",
                    new XAttribute("versao", "1.00"),
                    new XElement(nfe + "idLote", DateTime.Now.Ticks.ToString()),
                    XElement.Parse(xmlEvento)
                )
            );

            return xml.ToString();
        }

        private string CriarEnvelopeInutilizacao(string xmlInutilizacao)
        {
            // Para inutilização, o XML já está completo, não precisa de envelope adicional
            return xmlInutilizacao;
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

        public EventoRetorno ProcessarRetornoEvento(string xmlRetorno)
        {
            try
            {
                var doc = XDocument.Parse(xmlRetorno);
                var ns = doc.Root.GetDefaultNamespace();

                var retEvento = doc.Descendants(ns + "retEvento").FirstOrDefault();
                if (retEvento == null)
                {
                    throw new Exception("Retorno do evento não encontrado no XML");
                }

                var infEvento = retEvento.Element(ns + "infEvento");
                
                return new EventoRetorno
                {
                    TpAmb = infEvento.Element(ns + "tpAmb")?.Value,
                    VerAplic = infEvento.Element(ns + "verAplic")?.Value,
                    COrgao = infEvento.Element(ns + "cOrgao")?.Value,
                    CStat = infEvento.Element(ns + "cStat")?.Value,
                    XMotivo = infEvento.Element(ns + "xMotivo")?.Value,
                    ChNFe = infEvento.Element(ns + "chNFe")?.Value,
                    TpEvento = infEvento.Element(ns + "tpEvento")?.Value,
                    XEvento = infEvento.Element(ns + "xEvento")?.Value,
                    NSeqEvento = infEvento.Element(ns + "nSeqEvento")?.Value,
                    DhRegEvento = infEvento.Element(ns + "dhRegEvento")?.Value,
                    NProt = infEvento.Element(ns + "nProt")?.Value
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao processar retorno do evento: {ex.Message}", ex);
            }
        }
    }

    public class EventoRetorno
    {
        public string TpAmb { get; set; }
        public string VerAplic { get; set; }
        public string COrgao { get; set; }
        public string CStat { get; set; }
        public string XMotivo { get; set; }
        public string ChNFe { get; set; }
        public string TpEvento { get; set; }
        public string XEvento { get; set; }
        public string NSeqEvento { get; set; }
        public string DhRegEvento { get; set; }
        public string NProt { get; set; }

        public bool Autorizado => CStat == "135" || CStat == "136" || CStat == "155";
    }
}
