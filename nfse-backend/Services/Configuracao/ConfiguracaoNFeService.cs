using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace nfse_backend.Services.Configuracao
{
    public class ConfiguracaoNFeService
    {
        private readonly IConfiguration _configuration;
        private AmbienteNFe _ambienteAtual;
        private Dictionary<string, EmpresaConfig> _empresasConfig;

        public ConfiguracaoNFeService(IConfiguration configuration)
        {
            _configuration = configuration;
            _empresasConfig = new Dictionary<string, EmpresaConfig>();
            CarregarConfiguracoes();
        }

        private void CarregarConfiguracoes()
        {
            // Carregar ambiente padrão
            var ambiente = _configuration["NFe:Ambiente"] ?? "HOMOLOGACAO";
            _ambienteAtual = ambiente.ToUpper() == "PRODUCAO" ? AmbienteNFe.Producao : AmbienteNFe.Homologacao;
        }

        public AmbienteNFe ObterAmbienteAtual()
        {
            return _ambienteAtual;
        }

        public void AlterarAmbiente(AmbienteNFe ambiente)
        {
            _ambienteAtual = ambiente;
        }

        public EmpresaConfig ObterConfiguracaoEmpresa(string cnpj)
        {
            if (_empresasConfig.ContainsKey(cnpj))
            {
                return _empresasConfig[cnpj];
            }

            // Configuração padrão se não encontrar específica
            return new EmpresaConfig
            {
                CNPJ = cnpj,
                Ambiente = _ambienteAtual,
                CertificadoPath = _configuration[$"NFe:Empresas:{cnpj}:CertificadoPath"] ?? string.Empty,
                CertificadoSenha = _configuration[$"NFe:Empresas:{cnpj}:CertificadoSenha"] ?? string.Empty,
                SerieNFe = int.Parse(_configuration[$"NFe:Empresas:{cnpj}:SerieNFe"] ?? "1"),
                ProximoNumeroNFe = int.Parse(_configuration[$"NFe:Empresas:{cnpj}:ProximoNumeroNFe"] ?? "1"),
                CRT = int.Parse(_configuration[$"NFe:Empresas:{cnpj}:CRT"] ?? "3"), // 3 = Regime Normal
                RegimeTributario = _configuration[$"NFe:Empresas:{cnpj}:RegimeTributario"] ?? "LUCRO_PRESUMIDO"
            };
        }

        public void SalvarConfiguracaoEmpresa(EmpresaConfig config)
        {
            _empresasConfig[config.CNPJ] = config;
            // Em produção, salvar no banco de dados ou arquivo de configuração
        }

        public UrlsWebService ObterUrlsWebService(string uf, AmbienteNFe ambiente)
        {
            // 1) Tenta carregar de config/endpoints.json se existir
            try
            {
                var endpointsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", "endpoints.json");
                if (File.Exists(endpointsPath))
                {
                    var json = File.ReadAllText(endpointsPath);
                    var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json);
                    var key = uf.ToUpper();
                    var envKey = ambiente == AmbienteNFe.Producao ? "prod" : "homolog";
                    if (dict != null && dict.ContainsKey(key) && dict[key].ContainsKey(envKey))
                    {
                        var urlsJson = dict[key];
                        return new UrlsWebService
                        {
                            UrlNfeAutorizacao = urlsJson.GetValueOrDefault("autorizacao") ?? urlsJson.GetValueOrDefault(envKey) ?? string.Empty,
                            UrlNfeRetAutorizacao = urlsJson.GetValueOrDefault("retAutorizacao") ?? string.Empty,
                            UrlNfeConsultaProtocolo = urlsJson.GetValueOrDefault("consultaProtocolo") ?? string.Empty,
                            UrlNfeStatusServico = urlsJson.GetValueOrDefault("statusServico") ?? string.Empty,
                            UrlNfeInutilizacao = urlsJson.GetValueOrDefault("inutilizacao") ?? string.Empty,
                            UrlRecepcaoEvento = urlsJson.GetValueOrDefault("recepcaoEvento") ?? string.Empty,
                            UrlNfeDistribuicaoDFe = dict.GetValueOrDefault("BR")?.GetValueOrDefault("distribuicaoDFe") ?? string.Empty
                        };
                    }
                }
            }
            catch { }

            var urls = new UrlsWebService();
            var ambienteStr = ambiente == AmbienteNFe.Producao ? "PRODUCAO" : "HOMOLOGACAO";

            // URLs específicas por UF
            switch (uf.ToUpper())
            {
                case "SP":
                    if (ambiente == AmbienteNFe.Homologacao)
                    {
                        urls.UrlNfeAutorizacao = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx";
                        urls.UrlNfeRetAutorizacao = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx";
                        urls.UrlNfeConsultaProtocolo = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeconsultaprotocolo4.asmx";
                        urls.UrlNfeStatusServico = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfestatusservico4.asmx";
                        urls.UrlNfeInutilizacao = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nfeinutilizacao4.asmx";
                        urls.UrlRecepcaoEvento = "https://homologacao.nfe.fazenda.sp.gov.br/ws/nferecepcaoevento4.asmx";
                        urls.UrlNfeDistribuicaoDFe = "https://hom.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx";
                    }
                    else
                    {
                        urls.UrlNfeAutorizacao = "https://nfe.fazenda.sp.gov.br/ws/nfeautorizacao4.asmx";
                        urls.UrlNfeRetAutorizacao = "https://nfe.fazenda.sp.gov.br/ws/nferetautorizacao4.asmx";
                        urls.UrlNfeConsultaProtocolo = "https://nfe.fazenda.sp.gov.br/ws/nfeconsultaprotocolo4.asmx";
                        urls.UrlNfeStatusServico = "https://nfe.fazenda.sp.gov.br/ws/nfestatusservico4.asmx";
                        urls.UrlNfeInutilizacao = "https://nfe.fazenda.sp.gov.br/ws/nfeinutilizacao4.asmx";
                        urls.UrlRecepcaoEvento = "https://nfe.fazenda.sp.gov.br/ws/nferecepcaoevento4.asmx";
                        urls.UrlNfeDistribuicaoDFe = "https://www1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx";
                    }
                    break;

                case "RJ":
                case "ES":
                    // Estados que usam SVRS
                    if (ambiente == AmbienteNFe.Homologacao)
                    {
                        urls.UrlNfeAutorizacao = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx";
                        urls.UrlNfeRetAutorizacao = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx";
                        urls.UrlNfeConsultaProtocolo = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx";
                        urls.UrlNfeStatusServico = "https://nfe-homologacao.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx";
                        urls.UrlNfeInutilizacao = "https://nfe-homologacao.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx";
                        urls.UrlRecepcaoEvento = "https://nfe-homologacao.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx";
                    }
                    else
                    {
                        urls.UrlNfeAutorizacao = "https://nfe.svrs.rs.gov.br/ws/NfeAutorizacao/NFeAutorizacao4.asmx";
                        urls.UrlNfeRetAutorizacao = "https://nfe.svrs.rs.gov.br/ws/NfeRetAutorizacao/NFeRetAutorizacao4.asmx";
                        urls.UrlNfeConsultaProtocolo = "https://nfe.svrs.rs.gov.br/ws/NfeConsulta/NfeConsulta4.asmx";
                        urls.UrlNfeStatusServico = "https://nfe.svrs.rs.gov.br/ws/NfeStatusServico/NfeStatusServico4.asmx";
                        urls.UrlNfeInutilizacao = "https://nfe.svrs.rs.gov.br/ws/nfeinutilizacao/nfeinutilizacao4.asmx";
                        urls.UrlRecepcaoEvento = "https://nfe.svrs.rs.gov.br/ws/recepcaoevento/recepcaoevento4.asmx";
                    }
                    break;

                case "MG":
                    if (ambiente == AmbienteNFe.Homologacao)
                    {
                        urls.UrlNfeAutorizacao = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeAutorizacao4";
                        urls.UrlNfeRetAutorizacao = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeRetAutorizacao4";
                        urls.UrlNfeConsultaProtocolo = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeConsultaProtocolo4";
                        urls.UrlNfeStatusServico = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeStatusServico4";
                        urls.UrlNfeInutilizacao = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeInutilizacao4";
                        urls.UrlRecepcaoEvento = "https://hnfe.fazenda.mg.gov.br/nfe2/services/NFeRecepcaoEvento4";
                    }
                    else
                    {
                        urls.UrlNfeAutorizacao = "https://nfe.fazenda.mg.gov.br/nfe2/services/NFeAutorizacao4";
                        urls.UrlNfeRetAutorizacao = "https://nfe.fazenda.mg.gov.br/nfe2/services/NFeRetAutorizacao4";
                        urls.UrlNfeConsultaProtocolo = "https://nfe.fazenda.mg.gov.br/nfe2/services/NFeConsultaProtocolo4";
                        urls.UrlNfeStatusServico = "https://nfe.fazenda.mg.gov.br/nfe2/services/NFeStatusServico4";
                        urls.UrlNfeInutilizacao = "https://nfe.fazenda.mg.gov.br/nfe2/services/NFeInutilizacao4";
                        urls.UrlRecepcaoEvento = "https://nfe.fazenda.mg.gov.br/nfe2/services/NFeRecepcaoEvento4";
                    }
                    break;

                default:
                    // Usar SVRS como padrão para estados não configurados
                    return ObterUrlsWebService("ES", ambiente);
            }

            // URL de distribuição é nacional
            if (ambiente == AmbienteNFe.Homologacao)
            {
                urls.UrlNfeDistribuicaoDFe = "https://hom.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx";
            }
            else
            {
                urls.UrlNfeDistribuicaoDFe = "https://www1.nfe.fazenda.gov.br/NFeDistribuicaoDFe/NFeDistribuicaoDFe.asmx";
            }

            return urls;
        }

        public ValidacaoConfig ObterConfiguracoesValidacao()
        {
            return new ValidacaoConfig
            {
                ValidarSchema = bool.Parse(_configuration["NFe:Validacao:ValidarSchema"] ?? "true"),
                ValidarCertificado = bool.Parse(_configuration["NFe:Validacao:ValidarCertificado"] ?? "true"),
                ValidarRegrasNegocio = bool.Parse(_configuration["NFe:Validacao:ValidarRegrasNegocio"] ?? "true"),
                TimeoutWebService = int.Parse(_configuration["NFe:Validacao:TimeoutWebService"] ?? "100000"),
                TentativasReenvio = int.Parse(_configuration["NFe:Validacao:TentativasReenvio"] ?? "3"),
                IntervaloReenvio = int.Parse(_configuration["NFe:Validacao:IntervaloReenvio"] ?? "5000")
            };
        }

        public ContingenciaConfig ObterConfiguracoesContingencia()
        {
            return new ContingenciaConfig
            {
                TipoContingencia = _configuration["NFe:Contingencia:Tipo"] ?? "SVCAN",
                JustificativaContingencia = _configuration["NFe:Contingencia:Justificativa"] ?? "Problemas técnicos",
                DataHoraEntrada = DateTime.Parse(_configuration["NFe:Contingencia:DataHoraEntrada"] ?? DateTime.Now.ToString()),
                Ativa = bool.Parse(_configuration["NFe:Contingencia:Ativa"] ?? "false")
            };
        }

        public ArmazenamentoConfig ObterConfiguracoesArmazenamento()
        {
            return new ArmazenamentoConfig
            {
                DiretorioXmlAutorizado = _configuration["NFe:Armazenamento:DiretorioXmlAutorizado"] ?? "XMLs/Autorizados",
                DiretorioXmlRejeitado = _configuration["NFe:Armazenamento:DiretorioXmlRejeitado"] ?? "XMLs/Rejeitados",
                DiretorioXmlContingencia = _configuration["NFe:Armazenamento:DiretorioXmlContingencia"] ?? "XMLs/Contingencia",
                DiretorioPDF = _configuration["NFe:Armazenamento:DiretorioPDF"] ?? "PDFs",
                DiretorioBackup = _configuration["NFe:Armazenamento:DiretorioBackup"] ?? "Backup",
                TempoRetencao = int.Parse(_configuration["NFe:Armazenamento:TempoRetencaoDias"] ?? "1825"), // 5 anos
                CompactarArquivos = bool.Parse(_configuration["NFe:Armazenamento:CompactarArquivos"] ?? "true")
            };
        }
    }

    public enum AmbienteNFe
    {
        Producao = 1,
        Homologacao = 2
    }

    public class EmpresaConfig
    {
        public string CNPJ { get; set; } = string.Empty;
        public AmbienteNFe Ambiente { get; set; }
        public string CertificadoPath { get; set; } = string.Empty;
        public string CertificadoSenha { get; set; } = string.Empty;
        public int SerieNFe { get; set; }
        public int ProximoNumeroNFe { get; set; }
        public int CRT { get; set; } // Código de Regime Tributário
        public string RegimeTributario { get; set; } = string.Empty;
        public Dictionary<string, object> ConfiguracoesAdicionais { get; set; } = new Dictionary<string, object>();
    }

    public class UrlsWebService
    {
        public string UrlNfeAutorizacao { get; set; } = string.Empty;
        public string UrlNfeRetAutorizacao { get; set; } = string.Empty;
        public string UrlNfeConsultaProtocolo { get; set; } = string.Empty;
        public string UrlNfeStatusServico { get; set; } = string.Empty;
        public string UrlNfeInutilizacao { get; set; } = string.Empty;
        public string UrlRecepcaoEvento { get; set; } = string.Empty;
        public string UrlNfeDistribuicaoDFe { get; set; } = string.Empty;
        public string UrlNfeCadastro { get; set; } = string.Empty;
    }

    public class ValidacaoConfig
    {
        public bool ValidarSchema { get; set; }
        public bool ValidarCertificado { get; set; }
        public bool ValidarRegrasNegocio { get; set; }
        public int TimeoutWebService { get; set; }
        public int TentativasReenvio { get; set; }
        public int IntervaloReenvio { get; set; }
    }

    public class ContingenciaConfig
    {
        public string TipoContingencia { get; set; } = string.Empty;
        public string JustificativaContingencia { get; set; } = string.Empty;
        public DateTime DataHoraEntrada { get; set; }
        public bool Ativa { get; set; }
    }

    public class ArmazenamentoConfig
    {
        public string DiretorioXmlAutorizado { get; set; } = string.Empty;
        public string DiretorioXmlRejeitado { get; set; } = string.Empty;
        public string DiretorioXmlContingencia { get; set; } = string.Empty;
        public string DiretorioPDF { get; set; } = string.Empty;
        public string DiretorioBackup { get; set; } = string.Empty;
        public int TempoRetencao { get; set; } // Em dias
        public bool CompactarArquivos { get; set; }
    }
}
