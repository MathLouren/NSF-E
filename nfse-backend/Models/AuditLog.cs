using System;
using System.ComponentModel.DataAnnotations;

namespace nfse_backend.Models
{
    public class AuditLog
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public Guid UserId { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Action { get; set; } = string.Empty;
        
        [MaxLength(4000)]
        public string Details { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Campos adicionais para auditoria completa NFe
        [MaxLength(50)]
        public string TipoOperacao { get; set; } = string.Empty;
        
        public DateTime DataHora { get; set; } = DateTime.Now;
        
        [MaxLength(100)]
        public string? UsuarioId { get; set; }
        
        [MaxLength(14)]
        public string? CnpjEmpresa { get; set; }
        
        [MaxLength(44)]
        public string? ChaveAcesso { get; set; }
        
        [MaxLength(20)]
        public string Status { get; set; } = string.Empty;
        
        public long? DuracaoMs { get; set; }
        
        [MaxLength(45)]
        public string? EnderecoIP { get; set; }
        
        [MaxLength(500)]
        public string? UserAgent { get; set; }
        
        [MaxLength(10)]
        public string? CodigoResposta { get; set; }
        
        [MaxLength(1000)]
        public string? MensagemResposta { get; set; }
    }
}

