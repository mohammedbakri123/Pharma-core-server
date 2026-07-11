using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Application.Reports.Interfaces;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Generates business reports.
/// </summary>
[Route("reports")]
[Authorize]
[Tags("Reports")]
public class ReportsController : ApiControllerBase
{
    /// <summary>
    /// Dashboard report.
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IGetDashboardReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(from, to, cancellationToken);
        return MapServiceResult(result);
    }
}
