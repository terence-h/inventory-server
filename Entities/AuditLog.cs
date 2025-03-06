using System.ComponentModel.DataAnnotations.Schema;

namespace inventory_server.Entities;

[Table("audit_logs")]
public class AuditLog
{
    public required Guid AuditLogId { get; set; }
    public required int AuditTypeId { get; set; } 
    public required string AuditContent { get; set; }
    public required string ActionBy { get; set; }
    public DateTime Date { get; set; }
    public AuditType AuditType { get; set; }
}