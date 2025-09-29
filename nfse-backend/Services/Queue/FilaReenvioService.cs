using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using nfse_backend.Services.WebService;
using nfse_backend.Services.Armazenamento;
using nfse_backend.Models.NFe;
using System.Text.Json;
using System.IO;

namespace nfse_backend.Services.Queue
{
    /// <summary>
    /// Serviço de fila para reenvio automático de XMLs que falharam por problemas temporários
    /// </summary>
    public class FilaReenvioService : BackgroundService
    {
        private readonly ILogger<FilaReenvioService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly Queue<ItemFilaReenvio> _filaReenvio;
        private readonly Dictionary<string, DateTime> _ultimaTentativa;
        private readonly object _lockFila = new object();

        public FilaReenvioService(ILogger<FilaReenvioService> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _filaReenvio = new Queue<ItemFilaReenvio>();
            _ultimaTentativa = new Dictionary<string, DateTime>();
        }

        /// <summary>
        /// Adiciona um XML à fila de reenvio
        /// </summary>
        public void AdicionarNaFila(string xmlContent, string chaveAcesso, string cnpjEmitente, string tipoOperacao, string uf, bool homologacao, string motivoFalha)
        {
            var item = new ItemFilaReenvio
            {
                Id = Guid.NewGuid().ToString(),
                XmlContent = xmlContent,
                ChaveAcesso = chaveAcesso,
                CnpjEmitente = cnpjEmitente,
                TipoOperacao = tipoOperacao,
                UF = uf,
                Homologacao = homologacao,
                MotivoFalha = motivoFalha,
                DataAdicao = DateTime.Now,
                ProximaTentativa = DateTime.Now.AddMinutes(5), // Primeira tentativa em 5 minutos
                TentativasRealizadas = 0,
                MaxTentativas = 10
            };

            lock (_lockFila)
            {
                _filaReenvio.Enqueue(item);
                _logger.LogInformation($"Item adicionado à fila de reenvio: {chaveAcesso} - {tipoOperacao}");
            }
        }

        /// <summary>
        /// Processa a fila de reenvio em background
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de fila de reenvio iniciado");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessarFilaReenvio();
                    await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); // Processar a cada 2 minutos
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar fila de reenvio");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); // Aguardar mais tempo em caso de erro
                }
            }

            _logger.LogInformation("Serviço de fila de reenvio finalizado");
        }

        private async Task ProcessarFilaReenvio()
        {
        var itensParaProcessar = new List<ItemFilaReenvio>();
        var itensFalhaDefinitiva = new List<ItemFilaReenvio>();

        // Coletar itens prontos para reenvio
        lock (_lockFila)
        {
            var agora = DateTime.Now;
            var filaTemp = new Queue<ItemFilaReenvio>();

            while (_filaReenvio.Count > 0)
            {
                var item = _filaReenvio.Dequeue();

                if (item.ProximaTentativa <= agora && item.TentativasRealizadas < item.MaxTentativas)
                {
                    itensParaProcessar.Add(item);
                }
                else if (item.TentativasRealizadas < item.MaxTentativas)
                {
                    // Recolocar na fila se ainda não passou o tempo ou não excedeu tentativas
                    filaTemp.Enqueue(item);
                }
                else
                {
                    // Item excedeu tentativas - coletar para processar fora do lock
                    _logger.LogError($"Item {item.ChaveAcesso} excedeu número máximo de tentativas ({item.MaxTentativas})");
                    itensFalhaDefinitiva.Add(item);
                }
            }

            // Recolocar itens não processados
            while (filaTemp.Count > 0)
            {
                _filaReenvio.Enqueue(filaTemp.Dequeue());
            }
        }

        // Processar falhas definitivas fora do lock
        foreach (var item in itensFalhaDefinitiva)
        {
            await SalvarFalhaDefinitiva(item);
        }

            // Processar itens fora do lock
            foreach (var item in itensParaProcessar)
            {
                await ProcessarItemReenvio(item);
            }

            if (itensParaProcessar.Count > 0)
            {
                _logger.LogInformation($"Processados {itensParaProcessar.Count} itens da fila de reenvio");
            }
        }

        private async Task ProcessarItemReenvio(ItemFilaReenvio item)
        {
            using var scope = _serviceProvider.CreateScope();
            var webServiceClient = scope.ServiceProvider.GetRequiredService<SefazWebServiceClient>();
            var armazenamentoService = scope.ServiceProvider.GetRequiredService<ArmazenamentoSeguroService>();

            try
            {
                _logger.LogInformation($"Tentativa {item.TentativasRealizadas + 1}/{item.MaxTentativas} de reenvio para {item.ChaveAcesso}");

                string resultado = "";

                // Tentar reenvio baseado no tipo de operação
                switch (item.TipoOperacao.ToUpper())
                {
                    case "ENVIO_LOTE":
                        resultado = await webServiceClient.EnviarLoteNFe(item.XmlContent, item.UF, item.Homologacao);
                        break;

                    case "CONSULTA_RECIBO":
                        resultado = await webServiceClient.ConsultarRecibo(item.XmlContent, item.UF, item.Homologacao);
                        break;

                    case "EVENTO":
                        resultado = await webServiceClient.EnviarEvento(item.XmlContent, item.UF, item.Homologacao);
                        break;

                    default:
                        _logger.LogWarning($"Tipo de operação não suportado para reenvio: {item.TipoOperacao}");
                        return;
                }

                // Analisar resultado
                if (AnalisarSucesso(resultado))
                {
                    _logger.LogInformation($"Reenvio bem-sucedido para {item.ChaveAcesso} após {item.TentativasRealizadas + 1} tentativas");
                    
                    // Salvar resultado do reenvio
                    await armazenamentoService.SalvarXmlAutorizado(
                        item.XmlContent, 
                        item.ChaveAcesso, 
                        item.CnpjEmitente
                    );

                    // Item processado com sucesso - não recolocar na fila
                    return;
                }
                else
                {
                    // Falha - recolocar na fila com nova tentativa
                    item.TentativasRealizadas++;
                    item.UltimaTentativa = DateTime.Now;
                    item.ProximaTentativa = CalcularProximaTentativa(item.TentativasRealizadas);
                    item.UltimoErro = ExtractErrorFromResult(resultado);

                    lock (_lockFila)
                    {
                        _filaReenvio.Enqueue(item);
                    }

                    _logger.LogWarning($"Reenvio falhou para {item.ChaveAcesso}. Próxima tentativa: {item.ProximaTentativa}");
                }
            }
            catch (SefazIndisponivelException ex)
            {
                // SEFAZ ainda indisponível - agendar nova tentativa
                item.TentativasRealizadas++;
                item.UltimaTentativa = DateTime.Now;
                item.ProximaTentativa = CalcularProximaTentativa(item.TentativasRealizadas);
                item.UltimoErro = ex.Message;

                lock (_lockFila)
                {
                    _filaReenvio.Enqueue(item);
                }

                _logger.LogWarning($"SEFAZ ainda indisponível para {item.ChaveAcesso}. Tentativa {item.TentativasRealizadas}/{item.MaxTentativas}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro inesperado no reenvio de {item.ChaveAcesso}");
                
                // Recolocar na fila com incremento de tentativas
                item.TentativasRealizadas++;
                item.UltimoErro = ex.Message;
                item.ProximaTentativa = CalcularProximaTentativa(item.TentativasRealizadas);

                if (item.TentativasRealizadas < item.MaxTentativas)
                {
                    lock (_lockFila)
                    {
                        _filaReenvio.Enqueue(item);
                    }
                }
            }
        }

        private DateTime CalcularProximaTentativa(int numeroTentativa)
        {
            // Backoff exponencial: 5min, 10min, 20min, 40min, 1h20min, etc.
            var delayMinutos = Math.Min(5 * Math.Pow(2, numeroTentativa - 1), 480); // Máximo 8 horas
            return DateTime.Now.AddMinutes(delayMinutos);
        }

        private bool AnalisarSucesso(string resultado)
        {
            if (string.IsNullOrEmpty(resultado))
                return false;

            // Verificar códigos de sucesso comuns
            return resultado.Contains("cStat>100<") || // Autorizado
                   resultado.Contains("cStat>103<") || // Lote recebido
                   resultado.Contains("cStat>135<") || // Evento registrado
                   resultado.Contains("cStat>136<");   // Evento já registrado
        }

        private string ExtractErrorFromResult(string resultado)
        {
            try
            {
                // Extrair mensagem de erro do XML de retorno
                if (resultado.Contains("xMotivo>"))
                {
                    var inicio = resultado.IndexOf("xMotivo>") + 8;
                    var fim = resultado.IndexOf("</xMotivo>");
                    if (fim > inicio)
                    {
                        return resultado.Substring(inicio, fim - inicio);
                    }
                }
                
                return "Erro não identificado no retorno da SEFAZ";
            }
            catch
            {
                return "Erro ao extrair mensagem de erro";
            }
        }

        private async Task SalvarFalhaDefinitiva(ItemFilaReenvio item)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var armazenamentoService = scope.ServiceProvider.GetRequiredService<ArmazenamentoSeguroService>();

                var motivoCompleto = $"Falha definitiva após {item.TentativasRealizadas} tentativas. Último erro: {item.UltimoErro}";
                
                await armazenamentoService.SalvarXmlRejeitado(
                    item.XmlContent,
                    item.Id,
                    item.CnpjEmitente,
                    motivoCompleto
                );

                _logger.LogError($"XML salvo como falha definitiva: {item.ChaveAcesso}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao salvar falha definitiva para {item.ChaveAcesso}");
            }
        }

        /// <summary>
        /// Obtém estatísticas da fila de reenvio
        /// </summary>
        public FilaReenvioEstatisticas ObterEstatisticas()
        {
            lock (_lockFila)
            {
                var agora = DateTime.Now;
                var itens = _filaReenvio.ToArray();

                return new FilaReenvioEstatisticas
                {
                    TotalItens = itens.Length,
                    ItensAguardandoReenvio = itens.Count(i => i.ProximaTentativa > agora),
                    ItensProntosParaReenvio = itens.Count(i => i.ProximaTentativa <= agora),
                    MediaTentativasPorItem = itens.Length > 0 ? itens.Average(i => i.TentativasRealizadas) : 0,
                    ItemMaisAntigo = itens.OrderBy(i => i.DataAdicao).FirstOrDefault()?.DataAdicao,
                    UltimaExecucao = DateTime.Now
                };
            }
        }
    }

    /// <summary>
    /// Item da fila de reenvio
    /// </summary>
    public class ItemFilaReenvio
    {
        public string Id { get; set; } = "";
        public string XmlContent { get; set; } = "";
        public string ChaveAcesso { get; set; } = "";
        public string CnpjEmitente { get; set; } = "";
        public string TipoOperacao { get; set; } = "";
        public string UF { get; set; } = "";
        public bool Homologacao { get; set; }
        public string MotivoFalha { get; set; } = "";
        public DateTime DataAdicao { get; set; }
        public DateTime ProximaTentativa { get; set; }
        public DateTime? UltimaTentativa { get; set; }
        public int TentativasRealizadas { get; set; }
        public int MaxTentativas { get; set; }
        public string UltimoErro { get; set; } = "";
    }

    /// <summary>
    /// Estatísticas da fila de reenvio
    /// </summary>
    public class FilaReenvioEstatisticas
    {
        public int TotalItens { get; set; }
        public int ItensAguardandoReenvio { get; set; }
        public int ItensProntosParaReenvio { get; set; }
        public double MediaTentativasPorItem { get; set; }
        public DateTime? ItemMaisAntigo { get; set; }
        public DateTime UltimaExecucao { get; set; }
    }
}
