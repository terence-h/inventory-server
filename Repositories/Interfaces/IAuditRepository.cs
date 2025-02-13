using inventory_server.Models.Requests;
using inventory_server.Models.Response;

namespace inventory_server.Repositories.Interfaces;

public interface IAuditRepository
{
    Task<PagedResult<GetAuditLogResponse>> GetAuditLogsAsync(GetAuditLogsRequest filters);
    Task<Guid> CreateAuditLogAsync(AddAuditLogRequest request);
}