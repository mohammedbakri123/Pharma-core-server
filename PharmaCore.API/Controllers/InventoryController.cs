using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Inventory;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages pharmacy inventory and stock operations.
/// </summary>
[Route("inventory")]
[Authorize]
[Tags("Inventory")]
public class InventoryController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of stock items, optionally filtered by medicine.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="medicineId">Optional medicine ID filter.</param>
    /// <param name="service">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of stock items.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("stock")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetStock(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] int? medicineId = null,
        [FromServices] IGetStockService service = null!,
        CancellationToken cancellationToken = default)
    {
       
        var result = await service.ExecuteAsync(new GetStockQuery(page, limit, medicineId), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new
        {
            stock = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Returns stock details for a specific medicine including all batches.
    /// </summary>
    /// <param name="medicineId">The medicine ID.</param>
    /// <param name="service">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Stock details with batches.</response>
    /// <response code="404">Medicine not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("stock/{medicineId:int}")]
    [ProducesResponseType(typeof(StockWithBatchesDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetStockByMedicine(
        int medicineId,
        [FromServices] IGetStockByMedicineService service,
        CancellationToken cancellationToken)
    {
        var query = new GetStockByMedicineQuery(medicineId);
        var result = await service.ExecuteAsync(query, cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Returns a paginated list of stock alerts, optionally filtered by low-stock threshold, expiry days, or search term.
    /// Omitting both filters returns all stock with status labels.
    /// </summary>
    /// <param name="lowStockThreshold">Optional — only return items at or below this stock level.</param>
    /// <param name="expiringDays">Optional — only return items with batches expiring within this many days.</param>
    /// <param name="search">Optional — search by medicine name, Arabic name, or barcode.</param>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="service">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of stock alerts.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAlerts(
        [FromQuery] int? lowStockThreshold = null,
        [FromQuery] int? expiringDays = null,
        [FromQuery] string? search = null,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromServices] IStockAlertService service = null!,
        CancellationToken cancellationToken = default)
    {
        var query = new GetStockAlertQuery(lowStockThreshold, expiringDays, search, page, limit);
        var result = await service.ExecuteAsync(query, cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            items = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Creates a stock adjustment (increase or decrease).
    /// </summary>
    /// <param name="request">The adjustment data.</param>
    /// <param name="service">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The created adjustment.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpPost("adjust")]
    [ProducesResponseType(typeof(AdjustmentWithStockMovementDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Adjust(
        [FromBody] AdjustmentRequest request,
        [FromServices] ICreateAdjustmentService service,
        CancellationToken cancellationToken)
    {
        var command = new CreateAdjustmentCommand(
            request.MedicineId,
            request.BatchId,
            request.Quantity,
            request.Type,
            request.UserId,
            request.Reason);

        var result = await service.ExecuteAsync(command, cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Created($"/inventory/adjustments/{result.Data!.AdjustmentId}", result.Data);
    }
}
