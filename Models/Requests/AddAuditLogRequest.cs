using static inventory_server.Globals;

namespace inventory_server.Models.Requests;

public class AddAuditLogRequest
{
    public required AuditType AuditTypeId { get; set; } 
    public required string AuditContent { get; set; }
    public required string ActionBy { get; set; }
    public required DateTime Date { get; set; }
}