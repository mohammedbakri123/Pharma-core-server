using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Purchases;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;

namespace PharmaCore.API.Controllers;

[Route("purchases/{purchaseId:int}/returns")]
[Authorize]
[Tags("Purchase Returns")]
public class PurchaseReturnsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(PurchaseReturnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CreateReturn(
        int purchaseId,
        [FromBody] CreatePurchaseReturnRequest request,
        [FromServices] ICreatePurchaseReturnService createPurchaseReturnService,
        CancellationToken cancellationToken)
    {
        var result = await createPurchaseReturnService.ExecuteAsync(
            new CreatePurchaseReturnCommand(purchaseId, TryGetUserId(), request.Note),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return CreatedAtAction(nameof(GetReturnById), new { purchaseId, returnId = result.Data!.PurchaseReturnId }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListReturns(
        int purchaseId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromServices] IListPurchaseReturnsService listPurchaseReturnsService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listPurchaseReturnsService.ExecuteAsync(
            new ListPurchaseReturnsQuery(page, limit, purchaseId), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            purchaseId,
            returns = result.Data!.Items,
            pagination = new { total = result.Data.Total, page = result.Data.Page, limit = result.Data.Limit }
        });
    }

    [HttpGet("{returnId:int}")]
    [ProducesResponseType(typeof(PurchaseReturnDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReturnById(
        int returnId,
        [FromServices] IGetPurchaseReturnByIdService getPurchaseReturnByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getPurchaseReturnByIdService.ExecuteAsync(returnId, cancellationToken);
        return MapServiceResult(result);
    }
    //
    // [HttpPut("{returnId:int}")]
    // [ProducesResponseType(typeof(PurchaseReturnDto), StatusCodes.Status200OK)]
    // [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    // [ProducesResponseType(StatusCodes.Status404NotFound)]
    // public async Task<IActionResult> UpdateReturn(
    //     int returnId,
    //     [FromBody] UpdatePurchaseReturnRequest request,
    //     [FromServices] IUpdatePurchaseReturnService updatePurchaseReturnService,
    //     CancellationToken cancellationToken)
    // {
    //     var result = await updatePurchaseReturnService.ExecuteAsync(
    //         new UpdatePurchaseReturnCommand(returnId, request.Note), cancellationToken);
    //     return MapServiceResult(result);
    // }

    [HttpDelete("{returnId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReturn(
        int returnId,
        [FromServices] IDeletePurchaseReturnService deletePurchaseReturnService,
        CancellationToken cancellationToken)
    {
        var result = await deletePurchaseReturnService.ExecuteAsync(returnId, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return NoContent();
    }

    [HttpPost("{returnId:int}/items")]
    [ProducesResponseType(typeof(PurchaseReturnItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReturnItem(
        int returnId,
        int purchaseId,
        [FromBody] AddPurchaseReturnItemRequest request,
        [FromServices] IAddPurchaseReturnItemService addPurchaseReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await addPurchaseReturnItemService.ExecuteAsync(
            new AddPurchaseReturnItemCommand(returnId, request.PurchaseItemId, request.BatchId, request.Quantity, request.UnitPrice),
            cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Created($"/purchases/{purchaseId}/returns/{returnId}/items/{result.Data!.PurchaseReturnItemId}", result.Data);
    }

    [HttpPut("{returnId:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(PurchaseReturnItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReturnItem(
        int returnId,
        int itemId,
        [FromBody] UpdatePurchaseReturnItemRequest request,
        [FromServices] IUpdatePurchaseReturnItemService updatePurchaseReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await updatePurchaseReturnItemService.ExecuteAsync(
            new UpdatePurchaseReturnItemCommand(itemId, request.Quantity), cancellationToken);
        return MapServiceResult(result);
    }

    [HttpDelete("{returnId:int}/items/{itemId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReturnItem(
        int returnId,
        int itemId,
        [FromServices] IDeletePurchaseReturnItemService deletePurchaseReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await deletePurchaseReturnItemService.ExecuteAsync(
            new DeletePurchaseReturnItemCommand(itemId), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return NoContent();
    }

    [HttpPost("{returnId:int}/complete")]
    [ProducesResponseType(typeof(CompletePurchaseReturnResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteReturn(
        int returnId,
        [FromServices] ICompletePurchaseReturnService completePurchaseReturnService,
        CancellationToken cancellationToken)
    {
        var result = await completePurchaseReturnService.ExecuteAsync(returnId, cancellationToken);
        return MapServiceResult(result);
    }

    [HttpPost("{returnId:int}/cancel")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReturn(
        int returnId,
        [FromServices] ICancelPurchaseReturnService cancelPurchaseReturnService,
        CancellationToken cancellationToken)
    {
        var result = await cancelPurchaseReturnService.ExecuteAsync(returnId, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Purchase return cancelled" });
    }

    [HttpGet("{returnId:int}/balance")]
    [ProducesResponseType(typeof(PurchaseReturnBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnBalance(
        int returnId,
        [FromServices] IGetPurchaseReturnBalanceService getPurchaseReturnBalanceService,
        CancellationToken cancellationToken)
    {
        var result = await getPurchaseReturnBalanceService.ExecuteAsync(returnId, cancellationToken);
        return MapServiceResult(result);
    }

}
