using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Purchases;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Application.Purchases.Requests;
using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages purchases and purchase items.
/// </summary>
[Route("purchases")]
[Authorize]
[Tags("Purchases")]
public class PurchasesController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of purchases, optionally filtered by supplier, status, or date range.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] int? supplierId = null,
        [FromQuery] PurchaseStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromServices] IListPurchasesService listPurchasesService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listPurchasesService.ExecuteAsync(
            new ListPurchasesQuery(page, limit, supplierId, status, from, to), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            purchases = result.Data!.Items,
            pagination = new { total = result.Data.Total, page = result.Data.Page, limit = result.Data.Limit }
        });
    }

    /// <summary>
    /// Returns a single purchase by ID with all items.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetPurchaseByIdService getPurchaseByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getPurchaseByIdService.ExecuteAsync(new GetPurchaseByIdQuery(id), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Creates a new purchase (draft status).
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreatePurchaseRequest request,
        [FromServices] ICreatePurchaseService createPurchaseService,
        CancellationToken cancellationToken)
    {
        var result = await createPurchaseService.ExecuteAsync(
            new CreatePurchaseCommand(request.SupplierId, request.InvoiceNumber, request.Note),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Updates an existing purchase (supplier, invoice number, note).
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdatePurchaseRequest request,
        [FromServices] IUpdatePurchaseService updatePurchaseService,
        CancellationToken cancellationToken)
    {
        var result = await updatePurchaseService.ExecuteAsync(
            new UpdatePurchaseCommand(id, request.SupplierId, request.InvoiceNumber, request.Note, null),
            cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Soft-deletes a purchase and its items.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeletePurchaseService deletePurchaseService,
        CancellationToken cancellationToken)
    {
        var result = await deletePurchaseService.ExecuteAsync(id, cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Purchase deleted successfully" });
    }

    /// <summary>
    /// Adds an item to an existing purchase.
    /// </summary>
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(PurchaseItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
        int id,
        [FromBody] AddPurchaseItemRequest request,
        [FromServices] IAddPurchaseItemService addPurchaseItemService,
        CancellationToken cancellationToken)
    {
        var result = await addPurchaseItemService.ExecuteAsync(
            new AddPurchaseItemCommand(id, request.MedicineId, request.BatchId, request.Quantity,
                request.PurchasePrice, request.SellPrice, request.ExpireDate),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Updates an existing purchase item.
    /// </summary>
    [HttpPut("{id:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(PurchaseItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(
        int id,
        int itemId,
        [FromBody] UpdatePurchaseItemRequest request,
        [FromServices] IUpdatePurchaseItemService updatePurchaseItemService,
        CancellationToken cancellationToken)
    {
        var result = await updatePurchaseItemService.ExecuteAsync(
            new UpdatePurchaseItemCommand(id, itemId, request.Quantity, request.PurchasePrice, request.SellPrice, request.ExpireDate),
            cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Deletes an item from a purchase.
    /// </summary>
    [HttpDelete("{id:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        int id,
        int itemId,
        [FromServices] IDeletePurchaseItemService deletePurchaseItemService,
        CancellationToken cancellationToken)
    {
        var result = await deletePurchaseItemService.ExecuteAsync(id, itemId, cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Item deleted successfully" });
    }

    /// <summary>
    /// Completes a purchase: updates status, creates stock movements, and creates a payment IN.
    /// </summary>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(PurchaseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        int id,
        [FromServices] ICompletePurchaseService completePurchaseService,
        CancellationToken cancellationToken)
    {
        int? userId = TryGetUserId();
        var result = await completePurchaseService.ExecuteAsync(id, userId, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Cancels a purchase.
    /// </summary>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        int id,
        [FromServices] ICancelPurchaseService cancelPurchaseService,
        CancellationToken cancellationToken)
    {
        var result = await cancelPurchaseService.ExecuteAsync(id, cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Purchase cancelled successfully" });
    }

    private int? TryGetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }
}
