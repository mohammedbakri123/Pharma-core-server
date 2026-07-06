using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.SalesReturns;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;

namespace PharmaCore.API.Controllers;

[Route("sales/{saleId:int}/returns")]
[Authorize]
[Tags("Sales Returns")]
public class SalesReturnsController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SalesReturnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReturn(
        int saleId,
        [FromBody] CreateSalesReturnRequest request,
        [FromServices] ICreateSalesReturnService createSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await createSalesReturnService.ExecuteAsync(
            new CreateSalesReturnCommand(saleId, request.CustomerId, TryGetUserId(), request.Note),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return CreatedAtAction(nameof(GetReturnById), new { saleId, returnId = result.Data!.SalesReturnId }, result.Data);
    }

    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ListReturns(
        int saleId,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromServices] IListSalesReturnService listSalesReturnService = null!,
        CancellationToken cancellationToken = default)
    {
        var result = await listSalesReturnService.ExecuteAsync(
            new ListSalesReturnQuery(page, limit, saleId, null, null, null, null),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            saleId,
            returns = result.Data!.Items,
            pagination = new { total = result.Data.Total, page = result.Data.Page, limit = result.Data.Limit }
        });
    }

    [HttpGet("{returnId:int}")]
    [ProducesResponseType(typeof(SalesReturnDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReturnById(
        int saleId,
        int returnId,
        [FromServices] IGetSalesReturnByIdService getSalesReturnByIdService,
        CancellationToken cancellationToken)
    { 
        var result = await getSalesReturnByIdService.ExecuteAsync(new GetSalesReturnByIdQuery(saleId,returnId), cancellationToken);
        return MapServiceResult(result);
    }

    [HttpPut("{returnId:int}")]
    [ProducesResponseType(typeof(SalesReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReturn(
        int returnId,
        [FromBody] UpdateSalesReturnRequest request,
        [FromServices] IUpdateSalesReturnService updateSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await updateSalesReturnService.ExecuteAsync(new UpdateSalesReturnCommand(returnId, request.Note), cancellationToken);
        return MapServiceResult(result);
    }

    [HttpDelete("{returnId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReturn(
        int returnId,
        [FromServices] IDeleteSalesReturnService deleteSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await deleteSalesReturnService.ExecuteAsync(returnId, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return NoContent();
    }

    [HttpPost("{returnId:int}/items")]
    [ProducesResponseType(typeof(SalesReturnItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReturnItem(
        int saleId,
        int returnId,
        [FromBody] AddSalesReturnItemRequest request,
        [FromServices] IAddSalesReturnItemService addSalesReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await addSalesReturnItemService.ExecuteAsync(
            new AddSalesReturnItemCommand(returnId, request.SaleItemId, request.BatchId, request.Quantity, request.UnitPrice),
            cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Created($"/sales/{saleId}/returns/{returnId}/items/{result.Data!.SalesReturnItemId}", result.Data);
    }

    [HttpPut("{returnId:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(SalesReturnItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateReturnItem(
        int returnId,
        int itemId,
        [FromBody] UpdateSalesReturnItemRequest request,
        [FromServices] IUpdateSalesReturnItemService updateSalesReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await updateSalesReturnItemService.ExecuteAsync(new UpdateSalesReturnItemCommand(itemId, request.Quantity), cancellationToken);
        return MapServiceResult(result);
    }

    [HttpDelete("{returnId:int}/items/{itemId:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReturnItem(
        int returnId,
        int itemId,
        [FromServices] IDeleteSalesReturnItemService deleteSalesReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await deleteSalesReturnItemService.ExecuteAsync(new DeleteSalesReturnItemCommand(itemId), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return NoContent();
    }

    [HttpPost("{returnId:int}/complete")]
    [ProducesResponseType(typeof(CompleteSalesReturnResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompleteReturn(
        int returnId,
        [FromServices] ICompleteSalesReturnService completeSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await completeSalesReturnService.ExecuteAsync(returnId, cancellationToken);
        return MapServiceResult(result);
    }

    [HttpPost("{returnId:int}/cancel")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelReturn(
        int returnId,
        [FromServices] ICancelSalesReturnService cancelSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await cancelSalesReturnService.ExecuteAsync(returnId, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Sales return cancelled" });
    }

    [HttpGet("{returnId:int}/balance")]
    [ProducesResponseType(typeof(SalesReturnBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReturnBalance(
        int returnId,
        [FromServices] IGetSalesReturnBalanceService getSalesReturnBalanceService,
        CancellationToken cancellationToken)
    {
        var result = await getSalesReturnBalanceService.ExecuteAsync(returnId, cancellationToken);
        return MapServiceResult(result);
    }
}

