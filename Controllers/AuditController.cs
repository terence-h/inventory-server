using inventory_server.Models.Requests;
using inventory_server.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace inventory_server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditController(IAuditRepository auditRepository) : ControllerBase
{
    [HttpGet("getAuditLogs")]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? auditLogId,
        [FromQuery] int? auditTypeId,
        [FromQuery] string? actionBy,
        [FromQuery] DateTime? startDate,
        [FromQuery] DateTime? endDate,
        [FromQuery] int? page
        )
    {
        var request = new GetAuditLogsRequest
        {
            AuditLogId = auditLogId,
            AuditTypeId = auditTypeId,
            ActionBy = actionBy,
            StartDate = startDate,
            EndDate = endDate,
            Page = page
        };
        
        var response = await auditRepository.GetAuditLogsAsync(request);

        return Ok(response);
    }
}