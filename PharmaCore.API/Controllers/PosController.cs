using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Interfaces;
using PharmaCore.Application.POS.Requests;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Point-of-sale helpers: fast medicine search, barcode scan, and quick stock lookup.
/// </summary>
[Route("pos")]
[Authorize]
[Tags("POS")]
public class PosController : ApiControllerBase
{
    /// <summary>
    /// Fast search for medicines by name, Arabic name, or barcode.
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(List<PosMedicineDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string q,
        [FromServices] IPosSearchService posSearchService,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new { error = "Search query 'q' is required." });
        }

        var result = await posSearchService.ExecuteAsync(new PosSearchQuery(q), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Scan a barcode to get medicine details with price and stock.
    /// </summary>
    [HttpGet("scan/{barcode}")]
    [ProducesResponseType(typeof(PosMedicineDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Scan(
        string barcode,
        [FromServices] IPosScanService posScanService,
        CancellationToken cancellationToken)
    {
        var result = await posScanService.ExecuteAsync(new PosScanQuery(barcode), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Quick stock lookup for a medicine during sale.
    /// </summary>
    [HttpGet("quick-stock/{medicineId:int}")]
    [ProducesResponseType(typeof(PosStockDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuickStock(
        int medicineId,
        [FromServices] IPosStockService posStockService,
        CancellationToken cancellationToken)
    {
        var result = await posStockService.ExecuteAsync(new PosStockQuery(medicineId), cancellationToken);
        return MapServiceResult(result);
    }
}
