using AutoMapper;
using inventory_server.Database;
using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Repositories;

public class AuditRepository(AuditDbContext dbContext, IMapper mapper) : IAuditRepository
{
    public async Task<PagedResult<GetAuditLogResponse>> GetAuditLogsAsync(GetAuditLogsRequest filters)
    {
        IQueryable<AuditLog> auditLogs = dbContext.AuditLogs
            .Include(e => e.AuditType)
            .OrderByDescending(p => p.Date)
            .AsNoTracking();
        
        if (!string.IsNullOrEmpty(filters.AuditLogId))
        {
            auditLogs = auditLogs.Where(p => p.AuditLogId.ToString().Contains(filters.AuditLogId.ToLower()));
        }
        
        if (filters.AuditTypeId.HasValue)
        {
            auditLogs = auditLogs.Where(p => p.AuditTypeId == filters.AuditTypeId.Value);
        }
        
        if (!string.IsNullOrEmpty(filters.ActionBy))
        {
            auditLogs = auditLogs.Where(p => p.ActionBy.ToLower().Contains(filters.ActionBy.ToLower()));
        }
        
        if (filters.StartDate.HasValue || filters.EndDate.HasValue)
        {
            auditLogs = auditLogs.Where(p => (p.Date >= (filters.StartDate != null ? DateTime.SpecifyKind(filters.StartDate.Value, DateTimeKind.Utc) : DateTime.UnixEpoch)) && (p.Date <= (filters.EndDate != null ? DateTime.SpecifyKind(filters.EndDate.Value, DateTimeKind.Utc) : DateTime.MaxValue)));
        }
        
        // Pagination logic
        const int pageSize = 10;
        var totalCount = await auditLogs.CountAsync();
        var totalPages = Math.Max(1, (int)Math.Ceiling(totalCount / (double)pageSize));
        var page = Math.Clamp(filters.Page ?? 1, 1, totalPages);

        var pagedAuditLogs = await auditLogs
            .Select(e => new GetAuditLogResponse
            {
                AuditLogId = e.AuditLogId,
                AuditTypeId = e.AuditTypeId,
                AuditTypeName = e.AuditType.Name,
                AuditContent = e.AuditContent,
                ActionBy = e.ActionBy,
                Date = e.Date
            })
            .OrderByDescending(p => p.Date)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<GetAuditLogResponse>
        {
            CurrentPage = page,
            TotalPages = totalPages,
            Items = pagedAuditLogs
        };
    }

    public async Task<Guid> CreateAuditLogAsync(AddAuditLogRequest request)
    {
        var auditLog = mapper.Map<AuditLog>(request);
        var utcDate = auditLog.Date.AddHours(-8);
        auditLog.Date = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);

        dbContext.AuditLogs.Add(auditLog);

        var created = await dbContext.SaveChangesAsync();

        return auditLog.AuditLogId;
    }
}