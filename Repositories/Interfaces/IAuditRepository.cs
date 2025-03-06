using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server.Repositories.Interfaces;

public interface IAuditRepository
{
    Task<PagedResult<GetAuditLogResponse>> GetAuditLogsAsync(GetAuditLogsRequest filters);
    Task<IEnumerable<GetAuditLogByProductIdResponse>> GetAuditLogsByProductIdAsync(int productId, int year);
    Task<GetAuditLogResponse> GetAuditLogAsync(Guid auditId);
    Task<Guid> CreateAuditLogAsync(AddAuditLogRequest request);
    Task<IEnumerable<AuditType>> GetAuditTypesAsync();
}