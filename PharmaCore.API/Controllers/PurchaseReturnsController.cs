using System.Security.Claims;
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
        int? userId = TryGetUserId();

        var items = request.Items.Select(i => new CreatePurchaseReturnItemCommand(
            i.PurchaseItemId, i.BatchId, i.Quantity, i.UnitPrice)).ToList();

        var result = await createPurchaseReturnService.ExecuteAsync(
            new CreatePurchaseReturnCommand(purchaseId, userId, request.Note, items),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListReturns(
        int purchaseId,
        [FromServices] IListPurchaseReturnsService listPurchaseReturnsService,
        CancellationToken cancellationToken)
    {
        var result = await listPurchaseReturnsService.ExecuteAsync(
            new ListPurchaseReturnsQuery(purchaseId), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { purchaseId, returns = result.Data });
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

    private int? TryGetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }
}
