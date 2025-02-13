namespace inventory_server.Models.Requests;

public class GetAuditLogsRequest
{
    public string? AuditLogId;
    public int? AuditTypeId;
    public string? ActionBy;
    public DateTime? StartDate;
    public DateTime? EndDate;
    public int? Page;
}