using System.ComponentModel.DataAnnotations;

namespace nfse_backend.Models
{
    public class User
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;
        
        [Required]
        [EmailAddress]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [Required]
        public string PasswordHash { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(50)]
        public string Role { get; set; } = "Operator"; // Administrator, Operator, Auditor
        
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }
        
        // Navigation properties
        public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    }
    
    public static class UserRoles
    {
        public const string Administrator = "Administrator";
        public const string Operator = "Operator";
        public const string Auditor = "Auditor";
    }
}
