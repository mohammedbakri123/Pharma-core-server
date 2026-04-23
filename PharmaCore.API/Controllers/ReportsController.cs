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
    /// Daily sales report.
    /// </summary>
    [HttpGet("sales/daily")]
    [ProducesResponseType(typeof(DailySalesReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDailySales(
        [FromQuery] DateTime? date,
        [FromServices] IGetDailySalesReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(date, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Sales in date range.
    /// </summary>
    [HttpGet("sales/range")]
    [ProducesResponseType(typeof(SalesRangeReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSalesRange(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromServices] IGetSalesRangeReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(from, to, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Profit = sales revenue - cost.
    /// </summary>
    [HttpGet("profit")]
    [ProducesResponseType(typeof(ProfitReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProfit(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IGetProfitReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(from, to, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Stock overview report.
    /// </summary>
    [HttpGet("stock")]
    [ProducesResponseType(typeof(StockReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStock(
        [FromQuery] int? categoryId,
        [FromServices] IGetStockReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(categoryId, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Expired items report.
    /// </summary>
    [HttpGet("expired")]
    [ProducesResponseType(typeof(ExpiredReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpired(
        [FromQuery] DateTime? includeExpiredBefore,
        [FromServices] IGetExpiredReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(includeExpiredBefore, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Cash vs card summary.
    /// </summary>
    [HttpGet("payments")]
    [ProducesResponseType(typeof(PaymentsReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromServices] IGetPaymentsReportService service,
        CancellationToken cancellationToken)
    {
        var result = await service.ExecuteAsync(from, to, cancellationToken);
        return MapServiceResult(result);
    }
}
