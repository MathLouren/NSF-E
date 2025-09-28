using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using nfse_backend.Services.WebService;
using nfse_backend.Services.Configuracao;

namespace nfse_backend.Services.Monitoramento
{
    public class MonitoramentoService : BackgroundService
    {
        private readonly ILogger<MonitoramentoService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Dictionary<string, StatusSefaz> _statusSefaz;
        private readonly Timer _timerVerificacao;

        public MonitoramentoService(
            ILogger<MonitoramentoService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _statusSefaz = new Dictionary<string, StatusSefaz>();
            
            // Verificar status a cada 5 minutos
            _timerVerificacao = new Timer(VerificarStatusSefaz, null, TimeSpan.Zero, TimeSpan.FromMinutes(5));
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de monitoramento iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await MonitorarSistema();
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // Cancellation requested
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no monitoramento do sistema");
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                }
            }

            _logger.LogInformation("Serviço de monitoramento finalizado");
        }

        private async Task MonitorarSistema()
        {
            // Monitorar uso de memória
            var memoriaUsada = GC.GetTotalMemory(false) / 1024 / 1024; // MB
            if (memoriaUsada > 500) // Alerta se usar mais de 500MB
            {
                _logger.LogWarning($"Alto uso de memória detectado: {memoriaUsada}MB");
            }

            // Monitorar certificados próximos do vencimento
            await VerificarVencimentoCertificados();

            // Monitorar espaço em disco
            var espacoLivre = ObterEspacoLivreDisco();
            if (espacoLivre < 1024) // Alerta se menos de 1GB livre
            {
                _logger.LogWarning($"Pouco espaço em disco: {espacoLivre}MB livres");
            }
        }

        private async void VerificarStatusSefaz(object? state)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var webServiceClient = scope.ServiceProvider.GetRequiredService<SefazWebServiceClient>();
                var configuracaoService = scope.ServiceProvider.GetRequiredService<ConfiguracaoNFeService>();

                var ufsParaVerificar = new[] { "RJ", "SP", "SVRS" };
                var ambientes = new[] { true, false }; // homologação e produção

                foreach (var uf in ufsParaVerificar)
                {
                    foreach (var homologacao in ambientes)
                    {
                        var chave = $"{uf}_{(homologacao ? "HOM" : "PROD")}";
                        
                        try
                        {
                            var resposta = await webServiceClient.ConsultarStatusServico(uf, homologacao);
                            var disponivel = AnalisarRespostaStatus(resposta);
                            
                            var statusAnterior = _statusSefaz.ContainsKey(chave) ? _statusSefaz[chave].Disponivel : true;
                            
                            _statusSefaz[chave] = new StatusSefaz
                            {
                                UF = uf,
                                Ambiente = homologacao ? "Homologação" : "Produção",
                                Disponivel = disponivel,
                                UltimaVerificacao = DateTime.Now,
                                TempoResposta = TimeSpan.FromSeconds(1) // TODO: medir tempo real
                            };

                            // Log apenas mudanças de status
                            if (statusAnterior != disponivel)
                            {
                                var status = disponivel ? "DISPONÍVEL" : "INDISPONÍVEL";
                                _logger.LogWarning($"SEFAZ {uf} ({(homologacao ? "HOM" : "PROD")}) mudou para: {status}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Erro ao verificar SEFAZ {uf} ({(homologacao ? "HOM" : "PROD")})");
                            
                            _statusSefaz[chave] = new StatusSefaz
                            {
                                UF = uf,
                                Ambiente = homologacao ? "Homologação" : "Produção",
                                Disponivel = false,
                                UltimaVerificacao = DateTime.Now,
                                ErroUltimaVerificacao = ex.Message
                            };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro geral na verificação de status da SEFAZ");
            }
        }

        private async Task VerificarVencimentoCertificados()
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var certificadoService = scope.ServiceProvider.GetRequiredService<nfse_backend.Services.Certificado.CertificadoDigitalService>();
                
                var infoCert = certificadoService.ObterInformacoesCertificado();
                
                if (infoCert.ContainsKey("daysUntilExpiry"))
                {
                    var diasParaVencer = (int)infoCert["daysUntilExpiry"];
                    
                    if (diasParaVencer <= 7)
                    {
                        _logger.LogError($"URGENTE: Certificado digital vence em {diasParaVencer} dias!");
                    }
                    else if (diasParaVencer <= 30)
                    {
                        _logger.LogWarning($"Certificado digital vence em {diasParaVencer} dias!");
                    }
                }
                
                // Add await to make this actually async
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Erro ao verificar vencimento do certificado");
            }
        }

        private long ObterEspacoLivreDisco()
        {
            try
            {
                var drive = new System.IO.DriveInfo(AppDomain.CurrentDomain.BaseDirectory);
                return drive.AvailableFreeSpace / 1024 / 1024; // MB
            }
            catch
            {
                return long.MaxValue; // Se não conseguir verificar, assume que tem espaço
            }
        }

        private bool AnalisarRespostaStatus(string respostaXml)
        {
            try
            {
                var doc = new System.Xml.XmlDocument();
                doc.LoadXml(respostaXml);

                var nsManager = new System.Xml.XmlNamespaceManager(doc.NameTable);
                nsManager.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");

                var cStatNode = doc.SelectSingleNode("//nfe:cStat", nsManager);
                if (cStatNode != null)
                {
                    var cStat = cStatNode.InnerText;
                    return cStat == "107"; // Serviço em operação
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        public Dictionary<string, StatusSefaz> ObterStatusSefaz()
        {
            return new Dictionary<string, StatusSefaz>(_statusSefaz);
        }

        public bool SefazDisponivel(string uf, bool homologacao = true)
        {
            var chave = $"{uf}_{(homologacao ? "HOM" : "PROD")}";
            
            if (_statusSefaz.ContainsKey(chave))
            {
                var status = _statusSefaz[chave];
                
                // Considerar indisponível se última verificação foi há mais de 15 minutos
                if (DateTime.Now.Subtract(status.UltimaVerificacao).TotalMinutes > 15)
                {
                    return false;
                }
                
                return status.Disponivel;
            }
            
            // Se nunca verificou, assumir disponível
            return true;
        }

        public override void Dispose()
        {
            _timerVerificacao?.Dispose();
            base.Dispose();
        }
    }

    public class StatusSefaz
    {
        public string UF { get; set; } = "";
        public string Ambiente { get; set; } = "";
        public bool Disponivel { get; set; }
        public DateTime UltimaVerificacao { get; set; }
        public TimeSpan? TempoResposta { get; set; }
        public string ErroUltimaVerificacao { get; set; } = "";
    }
}
