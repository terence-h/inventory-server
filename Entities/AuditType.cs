using System.ComponentModel.DataAnnotations.Schema;

namespace inventory_server.Entities;

[Table("audit_types")]
public class AuditType
{
    public required int AuditTypeId { get; set; }
    public required string Name { get; set; }
    
    public ICollection<AuditLog> AuditLogs { get; set; }
}