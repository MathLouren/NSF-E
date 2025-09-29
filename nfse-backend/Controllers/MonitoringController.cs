using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using nfse_backend.Services.Monitoring;
using nfse_backend.Models;

namespace nfse_backend.Controllers
{
    /// <summary>
    /// Controller para monitoramento de WebServices da SEFAZ
    /// Baseado na documentação oficial da NF-e consultada via Context7
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MonitoringController : ControllerBase
    {
        private readonly WebServiceMonitor _webServiceMonitor;
        private readonly ILogger<MonitoringController> _logger;

        public MonitoringController(
            WebServiceMonitor webServiceMonitor,
            ILogger<MonitoringController> logger)
        {
            _webServiceMonitor = webServiceMonitor;
            _logger = logger;
        }

        /// <summary>
        /// Verifica disponibilidade de todos os WebServices de uma UF
        /// </summary>
        /// <param name="uf">Sigla da UF (ex: RJ, SP, MG)</param>
        /// <param name="homologacao">true para homologação, false para produção</param>
        /// <returns>Lista com status de todos os WebServices</returns>
        [HttpGet("webservices/{uf}")]
        public async Task<ActionResult<List<WebServiceStatus>>> VerificarWebServicesUF(
            string uf, 
            [FromQuery] bool homologacao = true)
        {
            try
            {
                _logger.LogInformation($"Verificando WebServices da UF {uf} - Ambiente: {(homologacao ? "Homologação" : "Produção")}");

                var resultados = await _webServiceMonitor.VerificarDisponibilidadeUFAsync(uf, homologacao);

                var response = new
                {
                    UF = uf.ToUpper(),
                    Ambiente = homologacao ? "Homologação" : "Produção",
                    DataHoraVerificacao = DateTime.Now,
                    TotalServicos = resultados.Count,
                    ServicosDisponiveis = resultados.Count(r => r.Disponivel),
                    ServicosIndisponiveis = resultados.Count(r => !r.Disponivel),
                    PercentualDisponibilidade = resultados.Count > 0 ? 
                        Math.Round((double)resultados.Count(r => r.Disponivel) / resultados.Count * 100, 2) : 0,
                    Servicos = resultados.OrderBy(r => r.NomeServico).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao verificar WebServices da UF {uf}");
                return StatusCode(500, new { 
                    error = "Erro interno do servidor", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Verifica status específico do serviço SEFAZ (através do próprio WebService)
        /// </summary>
        /// <param name="uf">Sigla da UF</param>
        /// <param name="homologacao">true para homologação, false para produção</param>
        /// <returns>Status detalhado do serviço SEFAZ</returns>
        [HttpGet("sefaz-status/{uf}")]
        public async Task<ActionResult<StatusServicoSefaz>> VerificarStatusSefaz(
            string uf,
            [FromQuery] bool homologacao = true)
        {
            try
            {
                _logger.LogInformation($"Consultando status da SEFAZ {uf} - Ambiente: {(homologacao ? "Homologação" : "Produção")}");

                var status = await _webServiceMonitor.VerificarStatusServicoSefazAsync(uf, homologacao);

                return Ok(new
                {
                    UF = uf.ToUpper(),
                    Ambiente = homologacao ? "Homologação" : "Produção",
                    DataHoraConsulta = DateTime.Now,
                    Status = status,
                    Interpretacao = new
                    {
                        StatusTexto = status.Disponivel ? "OPERACIONAL" : "INDISPONÍVEL",
                        Recomendacao = status.Disponivel ? 
                            "Serviço funcionando normalmente" : 
                            "Aguardar normalização ou usar contingência",
                        CodigoOficial = status.CodigoStatus,
                        DescricaoOficial = CodigosStatusNFe.ObterDescricao(status.CodigoStatus ?? "")
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao consultar status da SEFAZ {uf}");
                return StatusCode(500, new { 
                    error = "Erro interno do servidor", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Verifica disponibilidade de um WebService específico
        /// </summary>
        /// <param name="url">URL do WebService</param>
        /// <param name="nomeServico">Nome identificador do serviço</param>
        /// <returns>Status detalhado do WebService</returns>
        [HttpPost("webservice/verificar")]
        public async Task<ActionResult<WebServiceStatus>> VerificarWebService(
            [FromBody] VerificarWebServiceRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Url) || string.IsNullOrEmpty(request.NomeServico))
                {
                    return BadRequest(new { 
                        error = "URL e nome do serviço são obrigatórios" 
                    });
                }

                _logger.LogInformation($"Verificando WebService específico: {request.NomeServico} - {request.Url}");

                var status = await _webServiceMonitor.VerificarDisponibilidadeAsync(request.Url, request.NomeServico);

                return Ok(new
                {
                    DataHoraVerificacao = DateTime.Now,
                    Status = status,
                    Interpretacao = new
                    {
                        StatusTexto = status.Disponivel ? "DISPONÍVEL" : "INDISPONÍVEL",
                        TempoRespostaMs = status.TempoResposta.TotalMilliseconds,
                        QualidadeResposta = status.TempoResposta.TotalSeconds switch
                        {
                            < 1 => "EXCELENTE",
                            < 3 => "BOM", 
                            < 10 => "REGULAR",
                            _ => "LENTO"
                        },
                        WsdlStatus = status.WsdlValido ? "VÁLIDO" : "INVÁLIDO"
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao verificar WebService específico");
                return StatusCode(500, new { 
                    error = "Erro interno do servidor", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Dashboard consolidado com status de múltiplas UFs
        /// </summary>
        /// <param name="ufs">Lista de UFs para verificar (padrão: principais)</param>
        /// <param name="homologacao">true para homologação, false para produção</param>
        /// <returns>Dashboard consolidado</returns>
        [HttpGet("dashboard")]
        public async Task<ActionResult> Dashboard(
            [FromQuery] string[]? ufs = null,
            [FromQuery] bool homologacao = true)
        {
            try
            {
                // UFs padrão se não especificadas
                ufs ??= new[] { "RJ", "SP", "MG", "RS", "PR", "SC", "BA", "DF" };

                _logger.LogInformation($"Gerando dashboard para UFs: {string.Join(", ", ufs)} - Ambiente: {(homologacao ? "Homologação" : "Produção")}");

                var resultadosPorUF = new List<object>();
                var totalServicos = 0;
                var totalDisponiveis = 0;

                foreach (var uf in ufs)
                {
                    var resultados = await _webServiceMonitor.VerificarDisponibilidadeUFAsync(uf, homologacao);
                    var statusSefaz = await _webServiceMonitor.VerificarStatusServicoSefazAsync(uf, homologacao);

                    var disponiveis = resultados.Count(r => r.Disponivel);
                    var total = resultados.Count;

                    resultadosPorUF.Add(new
                    {
                        UF = uf,
                        TotalServicos = total,
                        ServicosDisponiveis = disponiveis,
                        PercentualDisponibilidade = total > 0 ? Math.Round((double)disponiveis / total * 100, 2) : 0,
                        StatusSefaz = statusSefaz.Disponivel ? "OPERACIONAL" : "INDISPONÍVEL",
                        CodigoStatusSefaz = statusSefaz.CodigoStatus,
                        VersaoAplicacao = statusSefaz.VersaoAplicacao,
                        UltimaAtualizacao = statusSefaz.DataHoraResposta
                    });

                    totalServicos += total;
                    totalDisponiveis += disponiveis;
                }

                var dashboard = new
                {
                    Ambiente = homologacao ? "Homologação" : "Produção",
                    DataHoraGeracao = DateTime.Now,
                    Resumo = new
                    {
                        TotalUFs = ufs.Length,
                        TotalServicos = totalServicos,
                        ServicosDisponiveis = totalDisponiveis,
                        ServicosIndisponiveis = totalServicos - totalDisponiveis,
                        PercentualGeralDisponibilidade = totalServicos > 0 ? 
                            Math.Round((double)totalDisponiveis / totalServicos * 100, 2) : 0,
                        UFsOperacionais = resultadosPorUF.Count(r => ((double)r.GetType().GetProperty("PercentualDisponibilidade")!.GetValue(r)!) > 80),
                        StatusGeral = totalServicos > 0 && (double)totalDisponiveis / totalServicos > 0.8 ? 
                            "ESTÁVEL" : "INSTÁVEL"
                    },
                    DetalhePorUF = resultadosPorUF.OrderBy(r => r.GetType().GetProperty("UF")!.GetValue(r)).ToList()
                };

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar dashboard de monitoramento");
                return StatusCode(500, new { 
                    error = "Erro interno do servidor", 
                    message = ex.Message 
                });
            }
        }

        /// <summary>
        /// Informações sobre códigos de status oficiais da NF-e
        /// </summary>
        /// <returns>Lista de códigos e descrições oficiais</returns>
        [HttpGet("codigos-status")]
        public ActionResult ObterCodigosStatus()
        {
            try
            {
                var codigos = CodigosStatusNFe.TodosCodigos
                    .Select(kvp => new
                    {
                        Codigo = kvp.Key,
                        Descricao = kvp.Value,
                        IsSucesso = CodigosStatusNFe.IsSucesso(kvp.Key),
                        IsRejeicao = CodigosStatusNFe.IsRejeicao(kvp.Key),
                        IsRecuperavel = CodigosStatusNFe.IsErroRecuperavel(kvp.Key),
                        RequerAcaoUsuario = CodigosStatusNFe.RequerAcaoUsuario(kvp.Key),
                        Categoria = CodigosStatusNFe.ObterCategoria(kvp.Key).ToString()
                    })
                    .OrderBy(c => c.Codigo)
                    .ToList();

                return Ok(new
                {
                    TotalCodigos = codigos.Count,
                    CodigosSucesso = codigos.Count(c => c.IsSucesso),
                    CodigosRejeicao = codigos.Count(c => c.IsRejeicao),
                    FonteDocumentacao = "Portal Nacional da NF-e - Documentação Oficial",
                    UltimaAtualizacao = "2025-09-28",
                    Codigos = codigos
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter códigos de status");
                return StatusCode(500, new { 
                    error = "Erro interno do servidor", 
                    message = ex.Message 
                });
            }
        }
    }

    /// <summary>
    /// Request para verificação de WebService específico
    /// </summary>
    public class VerificarWebServiceRequest
    {
        public string Url { get; set; } = string.Empty;
        public string NomeServico { get; set; } = string.Empty;
    }
}
