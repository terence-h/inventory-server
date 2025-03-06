using System.Text.Json;
using System.Text.RegularExpressions;
using AutoMapper;
using inventory_server.Database;
using inventory_server.Entities;
using inventory_server.Models.Requests;
using inventory_server.Models.Response;
using inventory_server.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace inventory_server.Repositories;

public class AuditRepository(AuditDbContext dbContext, AuditTypeDbContext typeDbContext, IMapper mapper) : IAuditRepository
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
        
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");

        var pagedAuditLogs = await auditLogs
            .Select(e => new GetAuditLogResponse
            {
                AuditLogId = e.AuditLogId,
                AuditTypeId = e.AuditTypeId,
                AuditTypeName = e.AuditType.Name,
                AuditContent = e.AuditContent,
                ActionBy = e.ActionBy,
                Date = TimeZoneInfo.ConvertTimeFromUtc(e.Date, targetTimeZone).ToString("dd/MM/yyyy HH:mm:ss")
            })
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
    
    public async Task<IEnumerable<GetAuditLogByProductIdResponse>> GetAuditLogsByProductIdAsync(int productId, int year)
    {
        IQueryable<AuditLog> auditLogs = dbContext.AuditLogs
            .Include(p => p.AuditType)
            .Where(p => p.Date.Year == year &&
                        (p.AuditTypeId == (int)Globals.AuditType.AddProduct ||
                         p.AuditTypeId == (int)Globals.AuditType.EditProduct) &&
                        p.AuditContent.Substring(15, 20).Contains($"ProductId:{productId}"))
            .OrderBy(p => p.Date)
            .AsNoTracking();

        // First, fetch the data asynchronously from the database
        var logsList = await auditLogs.ToListAsync();

        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
        
        var response = logsList.Select(e =>
        {
            int quantity = 0;
            
            var parts = e.AuditContent?.Split(',') ?? [];
            if (parts.Length > 5)
            {
                string quantityPart = parts[5];
                const string prefix = "Quantity:";
                if (quantityPart.StartsWith(prefix))
                {
                    // Extract the number after "Quantity:" and trim any whitespace
                    string numberPart = quantityPart.Substring(prefix.Length).Trim();
                    if (int.TryParse(numberPart, out var parsedValue))
                    {
                        quantity = parsedValue;
                    }
                }
            }
            
            string formattedDate = TimeZoneInfo
                .ConvertTimeFromUtc(e.Date, targetTimeZone)
                .ToString("dd/MM/yyyy HH:mm:ss");

            return new GetAuditLogByProductIdResponse
            {
                Quantity = quantity,
                Date = formattedDate
            };
        }).ToList();

        return response;
    }


    public async Task<GetAuditLogResponse> GetAuditLogAsync(Guid auditId)
    {
        var auditLog = await dbContext.AuditLogs
            .Include(p => p.AuditType)
            .FirstOrDefaultAsync(p => p.AuditLogId == auditId);
        
        if (auditLog == null)
        {
            return new GetAuditLogResponse();
        }
        
        var targetTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Singapore Standard Time");
        
        auditLog.AuditContent = FormatAuditContent(auditLog.AuditContent, (Globals.AuditType)auditLog.AuditTypeId);

        var auditLogResp = new GetAuditLogResponse()
        {
            AuditLogId = auditLog.AuditLogId,
            AuditTypeId = auditLog.AuditTypeId,
            AuditTypeName = auditLog.AuditType.Name,
            AuditContent = auditLog.AuditContent,
            ActionBy = auditLog.ActionBy,
            Date = TimeZoneInfo.ConvertTimeFromUtc(auditLog.Date, targetTimeZone).ToString("dd/MM/yyyy HH:mm:ss")
        };
        
        return auditLogResp;
    }

    public async Task<Guid> CreateAuditLogAsync(AddAuditLogRequest request)
    {
        var auditLog = mapper.Map<AuditLog>(request);
        var utcDate = auditLog.Date.AddHours(-8);
        auditLog.Date = DateTime.SpecifyKind(utcDate, DateTimeKind.Utc);

        dbContext.AuditLogs.Add(auditLog);

        await dbContext.SaveChangesAsync();

        return auditLog.AuditLogId;
    }

    public async Task<IEnumerable<AuditType>> GetAuditTypesAsync()
    {
        return await typeDbContext.AuditTypes.AsNoTracking().ToListAsync();
    }

    private static string FormatAuditContent(string content, Globals.AuditType type)
    {
        if (string.IsNullOrEmpty(content))
            return string.Empty;
        
        switch (type)
        {
            case Globals.AuditType.AddProduct:
            case Globals.AuditType.EditProduct:
            case Globals.AuditType.DeleteProduct:
            {
                if (content.StartsWith("["))
                {
                    content = Regex.Replace(content, @"\[[^\]]*\]", "");
                    var splitStr = content.Split(',');
                    content = string.Join("\n", splitStr);
                }
                break;
            }
            case Globals.AuditType.Register:
            case Globals.AuditType.Login:
            case Globals.AuditType.Logout:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }
        return content;
    }
}