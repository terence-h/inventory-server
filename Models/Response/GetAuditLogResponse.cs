namespace inventory_server.Models.Response;

public class GetAuditLogResponse
{
    public Guid AuditLogId { get; set; }
    public int AuditTypeId { get; set; } 
    public string AuditTypeName { get; set; }
    public string AuditContent { get; set; }
    public string ActionBy { get; set; }
    // public DateTime Date { get; set; }
    public string Date { get; set; }
}