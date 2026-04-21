using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.SalesReturns;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages sales returns operations.
/// </summary>
[Route("sales-returns")]
[Authorize]
[Tags("Sales Returns")]
public class SalesReturnsController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of sales returns, optionally filtered by sale, customer, user, or date range.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="saleId">Optional sale ID filter.</param>
    /// <param name="customerId">Optional customer ID filter.</param>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <param name="listSalesReturnService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of sales returns.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] int? saleId = null,
        [FromQuery] int? customerId = null,
        [FromQuery] int? userId = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromServices] IListSalesReturnService listSalesReturnService = null!,
        CancellationToken cancellationToken = default)
    {
        var result = await listSalesReturnService.ExecuteAsync(new ListSalesReturnQuery(page, limit, saleId, customerId, userId, from, to), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            salesReturns = result.Data!.Items,
            pagination = new { total = result.Data.Total, page = result.Data.Page, limit = result.Data.Limit }
        });
    }

    /// <summary>
    /// Returns a single sales return by ID with all details.
    /// </summary>
    /// <param name="id">Sales return ID.</param>
    /// <param name="getSalesReturnByIdService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sales return details.</response>
    /// <response code="404">Sales return not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SalesReturnDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetSalesReturnByIdService getSalesReturnByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getSalesReturnByIdService.ExecuteAsync(new GetSalesReturnByIdQuery(id), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Creates a new sales return. The creating user is automatically assigned.
    /// </summary>
    /// <param name="request">Create sales return request body.</param>
    /// <param name="createSalesReturnService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Sales return created successfully.</response>
    /// <response code="400">Validation error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SalesReturnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSalesReturnRequest request,
        [FromServices] ICreateSalesReturnService createSalesReturnService,
        CancellationToken cancellationToken)
    {
        int? userId = TryGetUserId();
        var result = await createSalesReturnService.ExecuteAsync(new CreateSalesReturnCommand(request.SaleId, request.CustomerId, userId, request.Note), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Updates a sales return (note only).
    /// </summary>
    /// <param name="id">Sales return ID.</param>
    /// <param name="request">Update sales return request body.</param>
    /// <param name="updateSalesReturnService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sales return updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Sales return not found.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SalesReturnDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSalesReturnRequest request,
        [FromServices] IUpdateSalesReturnService updateSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await updateSalesReturnService.ExecuteAsync(new UpdateSalesReturnCommand(id, request.Note), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Deletes a sales return (soft delete).
    /// </summary>
    /// <param name="id">Sales return ID.</param>
    /// <param name="deleteSalesReturnService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sales return deleted successfully.</response>
    /// <response code="404">Sales return not found.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteSalesReturnService deleteSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await deleteSalesReturnService.ExecuteAsync(id, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Sales return deleted" });
    }

    /// <summary>
    /// Adds an item to an existing sales return.
    /// </summary>
    /// <param name="id">Sales return ID.</param>
    /// <param name="request">Add sales return item request body.</param>
    /// <param name="addSalesReturnItemService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Item added successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Sales return not found.</response>
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(SalesReturnItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
        int id,
        [FromBody] AddSalesReturnItemRequest request,
        [FromServices] IAddSalesReturnItemService addSalesReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await addSalesReturnItemService.ExecuteAsync(new AddSalesReturnItemCommand(id, request.SaleItemId, request.BatchId, request.Quantity, request.UnitPrice), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Updates the quantity of an existing sales return item.
    /// </summary>
    /// <param name="id">Sales return ID.</param>
    /// <param name="itemId">Sales return item ID.</param>
    /// <param name="request">Update sales return item request body.</param>
    /// <param name="updateSalesReturnItemService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Item updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Sales return or item not found.</response>
    [HttpPut("{id:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(SalesReturnItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(
        int id,
        int itemId,
        [FromBody] UpdateSalesReturnItemRequest request,
        [FromServices] IUpdateSalesReturnItemService updateSalesReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await updateSalesReturnItemService.ExecuteAsync(new UpdateSalesReturnItemCommand(itemId, request.Quantity), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Removes an item from a sales return.
    /// </summary>
    /// <param name="id">Sales return ID.</param>
    /// <param name="itemId">Sales return item ID.</param>
    /// <param name="deleteSalesReturnItemService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Item removed successfully.</response>
    /// <response code="404">Sales return or item not found.</response>
    [HttpDelete("{id:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        int id,
        int itemId,
        [FromServices] IDeleteSalesReturnItemService deleteSalesReturnItemService,
        CancellationToken cancellationToken)
    {
        var result = await deleteSalesReturnItemService.ExecuteAsync(new DeleteSalesReturnItemCommand(itemId), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Item removed" });
    }

    private int? TryGetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }
}
