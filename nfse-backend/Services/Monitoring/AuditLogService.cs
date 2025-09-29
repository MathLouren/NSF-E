using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using nfse_backend.Data;
using nfse_backend.Models;

namespace nfse_backend.Services.Monitoring
{
    /// <summary>
    /// Serviço de auditoria e logs para rastreamento completo das operações do sistema NFe
    /// </summary>
    public class AuditLogService
    {
        private readonly ILogger<AuditLogService> _logger;
        private readonly AppDbContext _context;

        public AuditLogService(ILogger<AuditLogService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Registra uma operação de emissão de NFe
        /// </summary>
        public async Task RegistrarEmissaoNFe(AuditLogEmissaoNFe auditLog)
        {
            try
            {
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TipoOperacao = "EMISSAO_NFE",
                    DataHora = DateTime.Now,
                    UsuarioId = auditLog.UsuarioId,
                    CnpjEmpresa = auditLog.CnpjEmpresa,
                    ChaveAcesso = auditLog.ChaveAcesso,
                    Status = auditLog.Status,
                    Details = JsonSerializer.Serialize(auditLog.Detalhes),
                    DuracaoMs = auditLog.DuracaoMs,
                    EnderecoIP = auditLog.EnderecoIP,
                    UserAgent = auditLog.UserAgent,
                    CodigoResposta = auditLog.CodigoResposta,
                    MensagemResposta = auditLog.MensagemResposta
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                // Log estruturado para análise
                _logger.LogInformation("NFe emitida: {ChaveAcesso} - Status: {Status} - Duração: {Duracao}ms - CNPJ: {Cnpj}",
                    auditLog.ChaveAcesso, auditLog.Status, auditLog.DuracaoMs, auditLog.CnpjEmpresa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar audit log de emissão NFe: {ChaveAcesso}", auditLog.ChaveAcesso);
            }
        }

        /// <summary>
        /// Registra uma operação de cancelamento de NFe
        /// </summary>
        public async Task RegistrarCancelamentoNFe(AuditLogCancelamentoNFe auditLog)
        {
            try
            {
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TipoOperacao = "CANCELAMENTO_NFE",
                    DataHora = DateTime.Now,
                    UsuarioId = auditLog.UsuarioId,
                    CnpjEmpresa = auditLog.CnpjEmpresa,
                    ChaveAcesso = auditLog.ChaveAcesso,
                    Status = auditLog.Status,
                    Details = JsonSerializer.Serialize(auditLog),
                    DuracaoMs = auditLog.DuracaoMs,
                    EnderecoIP = auditLog.EnderecoIP,
                    UserAgent = auditLog.UserAgent,
                    CodigoResposta = auditLog.CodigoResposta,
                    MensagemResposta = auditLog.MensagemResposta
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogWarning("NFe cancelada: {ChaveAcesso} - Justificativa: {Justificativa} - CNPJ: {Cnpj}",
                    auditLog.ChaveAcesso, auditLog.Justificativa, auditLog.CnpjEmpresa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar audit log de cancelamento NFe: {ChaveAcesso}", auditLog.ChaveAcesso);
            }
        }

        /// <summary>
        /// Registra acesso ao sistema
        /// </summary>
        public async Task RegistrarAcesso(AuditLogAcesso auditLog)
        {
            try
            {
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TipoOperacao = auditLog.TipoAcesso,
                    DataHora = DateTime.Now,
                    UsuarioId = auditLog.UsuarioId,
                    Status = auditLog.Sucesso ? "SUCESSO" : "FALHA",
                    Details = JsonSerializer.Serialize(auditLog),
                    EnderecoIP = auditLog.EnderecoIP,
                    UserAgent = auditLog.UserAgent,
                    MensagemResposta = auditLog.MensagemErro
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                if (auditLog.Sucesso)
                {
                    _logger.LogInformation("Acesso realizado: {Usuario} - IP: {IP} - Tipo: {Tipo}",
                        auditLog.UsuarioId, auditLog.EnderecoIP, auditLog.TipoAcesso);
                }
                else
                {
                    _logger.LogWarning("Tentativa de acesso falhada: {Usuario} - IP: {IP} - Erro: {Erro}",
                        auditLog.UsuarioId, auditLog.EnderecoIP, auditLog.MensagemErro);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar audit log de acesso: {Usuario}", auditLog.UsuarioId);
            }
        }

        /// <summary>
        /// Registra operação de contingência
        /// </summary>
        public async Task RegistrarContingencia(AuditLogContingencia auditLog)
        {
            try
            {
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TipoOperacao = "CONTINGENCIA",
                    DataHora = DateTime.Now,
                    UsuarioId = auditLog.UsuarioId,
                    CnpjEmpresa = auditLog.CnpjEmpresa,
                    ChaveAcesso = auditLog.ChaveAcesso,
                    Status = auditLog.Status,
                    Details = JsonSerializer.Serialize(auditLog),
                    DuracaoMs = auditLog.DuracaoMs,
                    EnderecoIP = auditLog.EnderecoIP,
                    CodigoResposta = auditLog.CodigoResposta,
                    MensagemResposta = auditLog.MensagemResposta
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogWarning("Operação de contingência: {ChaveAcesso} - Tipo: {Tipo} - Status: {Status} - CNPJ: {Cnpj}",
                    auditLog.ChaveAcesso, auditLog.TipoContingencia, auditLog.Status, auditLog.CnpjEmpresa);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar audit log de contingência: {ChaveAcesso}", auditLog.ChaveAcesso);
            }
        }

        /// <summary>
        /// Registra operações de certificado digital
        /// </summary>
        public async Task RegistrarOperacaoCertificado(AuditLogCertificado auditLog)
        {
            try
            {
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TipoOperacao = "CERTIFICADO",
                    DataHora = DateTime.Now,
                    UsuarioId = auditLog.UsuarioId,
                    CnpjEmpresa = auditLog.CnpjEmpresa,
                    Status = auditLog.Status,
                    Details = JsonSerializer.Serialize(auditLog),
                    EnderecoIP = auditLog.EnderecoIP,
                    MensagemResposta = auditLog.MensagemErro
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                if (auditLog.Status == "SUCESSO")
                {
                    _logger.LogInformation("Certificado carregado: {Thumbprint} - Tipo: {Tipo} - Válido até: {ValidoAte} - CNPJ: {Cnpj}",
                        auditLog.Thumbprint, auditLog.TipoCertificado, auditLog.ValidoAte, auditLog.CnpjEmpresa);
                }
                else
                {
                    _logger.LogError("Erro no certificado: {Thumbprint} - Erro: {Erro} - CNPJ: {Cnpj}",
                        auditLog.Thumbprint, auditLog.MensagemErro, auditLog.CnpjEmpresa);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar audit log de certificado: {Thumbprint}", auditLog.Thumbprint);
            }
        }

        /// <summary>
        /// Registra falhas de comunicação com SEFAZ
        /// </summary>
        public async Task RegistrarFalhaSefaz(AuditLogFalhaSefaz auditLog)
        {
            try
            {
                var log = new AuditLog
                {
                    Id = Guid.NewGuid(),
                    TipoOperacao = "FALHA_SEFAZ",
                    DataHora = DateTime.Now,
                    UsuarioId = auditLog.UsuarioId,
                    CnpjEmpresa = auditLog.CnpjEmpresa,
                    ChaveAcesso = auditLog.ChaveAcesso,
                    Status = "FALHA",
                    Details = JsonSerializer.Serialize(auditLog),
                    DuracaoMs = auditLog.DuracaoMs,
                    CodigoResposta = auditLog.CodigoErro,
                    MensagemResposta = auditLog.MensagemErro
                };

                _context.AuditLogs.Add(log);
                await _context.SaveChangesAsync();

                _logger.LogError("Falha SEFAZ: {UF} - {Operacao} - {Erro} - Tentativa {Tentativa}/{MaxTentativas}",
                    auditLog.UF, auditLog.TipoOperacao, auditLog.MensagemErro, auditLog.TentativaAtual, auditLog.MaxTentativas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao registrar audit log de falha SEFAZ");
            }
        }

        /// <summary>
        /// Obtém estatísticas de auditoria para dashboard
        /// </summary>
        public AuditStatistics ObterEstatisticasAuditoria(DateTime dataInicio, DateTime dataFim, string? cnpjEmpresa = null)
        {
            try
            {
                var query = _context.AuditLogs.Where(a => a.DataHora >= dataInicio && a.DataHora <= dataFim);
                
                if (!string.IsNullOrEmpty(cnpjEmpresa))
                {
                    query = query.Where(a => a.CnpjEmpresa == cnpjEmpresa);
                }

                var logs = query.ToList();

                var stats = new AuditStatistics
                {
                    PeriodoInicio = dataInicio,
                    PeriodoFim = dataFim,
                    TotalOperacoes = logs.Count,
                    OperacoesPorTipo = logs.GroupBy(l => l.TipoOperacao)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    OperacoesPorStatus = logs.GroupBy(l => l.Status)
                        .ToDictionary(g => g.Key ?? "DESCONHECIDO", g => g.Count()),
                    TempoMedioOperacao = logs.Where(l => l.DuracaoMs.HasValue)
                        .Select(l => (double)l.DuracaoMs!.Value)
                        .DefaultIfEmpty(0)
                        .Average(),
                    OperacoesPorHora = logs.GroupBy(l => l.DataHora.Hour)
                        .ToDictionary(g => g.Key, g => g.Count()),
                    TopUsuarios = logs.Where(l => !string.IsNullOrEmpty(l.UsuarioId))
                        .GroupBy(l => l.UsuarioId)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .ToDictionary(g => g.Key!, g => g.Count()),
                    TopEmpresas = logs.Where(l => !string.IsNullOrEmpty(l.CnpjEmpresa))
                        .GroupBy(l => l.CnpjEmpresa)
                        .OrderByDescending(g => g.Count())
                        .Take(10)
                        .ToDictionary(g => g.Key!, g => g.Count())
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas de auditoria");
                return new AuditStatistics
                {
                    PeriodoInicio = dataInicio,
                    PeriodoFim = dataFim,
                    TotalOperacoes = 0
                };
            }
        }

        /// <summary>
        /// Busca logs de auditoria com filtros
        /// </summary>
        public List<AuditLog> BuscarLogsAuditoria(FiltroAuditLog filtro)
        {
            try
            {
                var query = _context.AuditLogs.AsQueryable();

                if (filtro.DataInicio.HasValue)
                    query = query.Where(a => a.DataHora >= filtro.DataInicio.Value);

                if (filtro.DataFim.HasValue)
                    query = query.Where(a => a.DataHora <= filtro.DataFim.Value);

                if (!string.IsNullOrEmpty(filtro.TipoOperacao))
                    query = query.Where(a => a.TipoOperacao == filtro.TipoOperacao);

                if (!string.IsNullOrEmpty(filtro.Status))
                    query = query.Where(a => a.Status == filtro.Status);

                if (!string.IsNullOrEmpty(filtro.UsuarioId))
                    query = query.Where(a => a.UsuarioId == filtro.UsuarioId);

                if (!string.IsNullOrEmpty(filtro.CnpjEmpresa))
                    query = query.Where(a => a.CnpjEmpresa == filtro.CnpjEmpresa);

                if (!string.IsNullOrEmpty(filtro.ChaveAcesso))
                    query = query.Where(a => a.ChaveAcesso == filtro.ChaveAcesso);

                if (!string.IsNullOrEmpty(filtro.EnderecoIP))
                    query = query.Where(a => a.EnderecoIP == filtro.EnderecoIP);

                return query.OrderByDescending(a => a.DataHora)
                    .Skip(filtro.Skip)
                    .Take(filtro.Take)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar logs de auditoria");
                return new List<AuditLog>();
            }
        }

        /// <summary>
        /// Remove logs antigos baseado na política de retenção
        /// </summary>
        public async Task<int> LimparLogsAntigos(int diasRetencao = 365)
        {
            try
            {
                var dataLimite = DateTime.Now.AddDays(-diasRetencao);
                var logsAntigos = _context.AuditLogs.Where(a => a.DataHora < dataLimite);
                var count = logsAntigos.Count();

                _context.AuditLogs.RemoveRange(logsAntigos);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Removidos {Count} logs de auditoria anteriores a {DataLimite}", count, dataLimite);
                return count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao limpar logs antigos de auditoria");
                return 0;
            }
        }
    }

    #region DTOs de Audit Log

    public class AuditLogEmissaoNFe
    {
        public string UsuarioId { get; set; } = "";
        public string CnpjEmpresa { get; set; } = "";
        public string ChaveAcesso { get; set; } = "";
        public string Status { get; set; } = "";
        public object Detalhes { get; set; } = new { };
        public long DuracaoMs { get; set; }
        public string EnderecoIP { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string CodigoResposta { get; set; } = "";
        public string MensagemResposta { get; set; } = "";
    }

    public class AuditLogCancelamentoNFe
    {
        public string UsuarioId { get; set; } = "";
        public string CnpjEmpresa { get; set; } = "";
        public string ChaveAcesso { get; set; } = "";
        public string Status { get; set; } = "";
        public string Justificativa { get; set; } = "";
        public long DuracaoMs { get; set; }
        public string EnderecoIP { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string CodigoResposta { get; set; } = "";
        public string MensagemResposta { get; set; } = "";
    }

    public class AuditLogAcesso
    {
        public string UsuarioId { get; set; } = "";
        public string TipoAcesso { get; set; } = ""; // LOGIN, LOGOUT, API_ACCESS
        public bool Sucesso { get; set; }
        public string EnderecoIP { get; set; } = "";
        public string UserAgent { get; set; } = "";
        public string MensagemErro { get; set; } = "";
    }

    public class AuditLogContingencia
    {
        public string UsuarioId { get; set; } = "";
        public string CnpjEmpresa { get; set; } = "";
        public string ChaveAcesso { get; set; } = "";
        public string TipoContingencia { get; set; } = "";
        public string Status { get; set; } = "";
        public long DuracaoMs { get; set; }
        public string EnderecoIP { get; set; } = "";
        public string CodigoResposta { get; set; } = "";
        public string MensagemResposta { get; set; } = "";
    }

    public class AuditLogCertificado
    {
        public string UsuarioId { get; set; } = "";
        public string CnpjEmpresa { get; set; } = "";
        public string Thumbprint { get; set; } = "";
        public string TipoCertificado { get; set; } = ""; // A1, A3
        public string Status { get; set; } = "";
        public DateTime? ValidoAte { get; set; }
        public string EnderecoIP { get; set; } = "";
        public string MensagemErro { get; set; } = "";
    }

    public class AuditLogFalhaSefaz
    {
        public string UsuarioId { get; set; } = "";
        public string CnpjEmpresa { get; set; } = "";
        public string ChaveAcesso { get; set; } = "";
        public string UF { get; set; } = "";
        public string TipoOperacao { get; set; } = "";
        public string CodigoErro { get; set; } = "";
        public string MensagemErro { get; set; } = "";
        public int TentativaAtual { get; set; }
        public int MaxTentativas { get; set; }
        public long DuracaoMs { get; set; }
    }

    public class FiltroAuditLog
    {
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public string? TipoOperacao { get; set; }
        public string? Status { get; set; }
        public string? UsuarioId { get; set; }
        public string? CnpjEmpresa { get; set; }
        public string? ChaveAcesso { get; set; }
        public string? EnderecoIP { get; set; }
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 100;
    }

    public class AuditStatistics
    {
        public DateTime PeriodoInicio { get; set; }
        public DateTime PeriodoFim { get; set; }
        public int TotalOperacoes { get; set; }
        public Dictionary<string, int> OperacoesPorTipo { get; set; } = new();
        public Dictionary<string, int> OperacoesPorStatus { get; set; } = new();
        public double TempoMedioOperacao { get; set; }
        public Dictionary<int, int> OperacoesPorHora { get; set; } = new();
        public Dictionary<string, int> TopUsuarios { get; set; } = new();
        public Dictionary<string, int> TopEmpresas { get; set; } = new();
    }

    #endregion
}
