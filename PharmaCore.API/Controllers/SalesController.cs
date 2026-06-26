using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Sales;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.API.Contracts.SalesReturns;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages sales and point-of-sale operations.
/// </summary>
[Route("sales")]
[Authorize]
[Tags("Sales")]
public class SalesController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of sales, optionally filtered by customer, user, status, or date range.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="customerId">Optional customer ID filter.</param>
    /// <param name="userId">Optional user ID filter.</param>
    /// <param name="status">Optional status filter (Pending, Completed, Cancelled).</param>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <param name="listSalesService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of sales.</response>
    /// <response code="400">Validation error.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] int? customerId = null,
        [FromQuery] int? userId = null,
        [FromQuery] SaleStatus? status = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromServices] IListSalesService listSalesService = null!,
        CancellationToken cancellationToken = default)
    {
        var result = await listSalesService.ExecuteAsync(new ListSalesQuery(page, limit, customerId, userId, status, from, to), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            sales = result.Data!.Items,
            pagination = new { total = result.Data.Total, page = result.Data.Page, limit = result.Data.Limit }
        });
    }

    /// <summary>
    /// Returns a single sale by ID with all details.
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="getSaleByIdService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sale details.</response>
    /// <response code="404">Sale not found.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SaleDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetSaleByIdService getSaleByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getSaleByIdService.ExecuteAsync(new GetSaleByIdQuery(id), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Creates a new sale (draft/pending status). The creating user is automatically assigned.
    /// </summary>
    /// <param name="request">Create sale request body.</param>
    /// <param name="createSaleService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Sale created successfully.</response>
    /// <response code="400">Validation error.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SaleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSaleRequest request,
        [FromServices] ICreateSaleService createSaleService,
        CancellationToken cancellationToken)
    {
        int? userId = TryGetUserId();
        var result = await createSaleService.ExecuteAsync(new CreateSaleCommand(userId, request.CustomerId, request.Note), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Adds an item (medicine) to an existing sale.
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="request">Add sale item request body.</param>
    /// <param name="addSaleItemService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">Item added successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Sale not found.</response>
    [HttpPost("{id:int}/items")]
    [ProducesResponseType(typeof(IEnumerable<SaleItemDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
        int id,
        [FromBody] AddSaleItemRequest request,
        [FromServices] IAddSaleItemService addSaleItemService,
        CancellationToken cancellationToken)
    {
        var result = await addSaleItemService.ExecuteAsync(new AddSaleItemCommand(id, request.MedicineId, request.Quantity), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Updates the quantity of an existing sale item.
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="itemId">Sale item ID.</param>
    /// <param name="request">Update sale item request body.</param>
    /// <param name="updateSaleItemService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Item updated successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Sale or item not found.</response>
    [HttpPut("{id:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(SaleItemDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItem(
        int id,
        int itemId,
        [FromBody] UpdateSaleItemRequest request,
        [FromServices] IUpdateSaleItemService updateSaleItemService,
        CancellationToken cancellationToken)
    {
        var result = await updateSaleItemService.ExecuteAsync(new UpdateSaleItemCommand(id, itemId, request.Quantity), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Removes an item from a sale.
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="itemId">Sale item ID.</param>
    /// <param name="deleteSaleItemService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Item removed successfully.</response>
    /// <response code="404">Sale or item not found.</response>
    [HttpDelete("{id:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteItem(
        int id,
        int itemId,
        [FromServices] IDeleteSaleItemService deleteSaleItemService,
        CancellationToken cancellationToken)
    {
        var result = await deleteSaleItemService.ExecuteAsync(new DeleteSaleItemCommand(id, itemId), cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Item removed" });
    }

    /// <summary>
    /// Completes a sale by updating inventory. The completing user is automatically assigned.
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="completeSaleService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sale completed successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Sale not found.</response>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(CompleteSaleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        int id,
        [FromServices] ICompleteSaleService completeSaleService,
        CancellationToken cancellationToken)
    {
        int? userId = TryGetUserId();
        var result = await completeSaleService.ExecuteAsync(new CompleteSaleCommand(id, userId), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Cancels a sale (reverts inventory, processes refunds if needed).
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="cancelSaleService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sale cancelled successfully.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Sale not found.</response>
    [HttpPost("{id:int}/cancel")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(
        int id,
        [FromServices] ICancelSaleService cancelSaleService,
        CancellationToken cancellationToken)
    {
        var result = await cancelSaleService.ExecuteAsync(id, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Sale cancelled" });
    }

    /// <summary>
    /// Returns the balance remaining on a sale (total minus payments).
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="getSaleBalanceService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sale balance details.</response>
    /// <response code="404">Sale not found.</response>
    [HttpGet("{id:int}/balance")]
    [ProducesResponseType(typeof(SaleBalanceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Balance(
        int id,
        [FromServices] IGetSaleBalanceService getSaleBalanceService,
        CancellationToken cancellationToken)
    {
        var result = await getSaleBalanceService.ExecuteAsync(id, cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Creates a new sales return for a sale.
    /// </summary>
    [HttpPost("{saleId:int}/return")]
    [ProducesResponseType(typeof(SalesReturnDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateReturn(
        int saleId,
        [FromBody] CreateSalesReturnRequest request,
        [FromServices] ICreateSalesReturnService createSalesReturnService,
        CancellationToken cancellationToken)
    {
        int? userId = TryGetUserId();
        var result = await createSalesReturnService.ExecuteAsync(
            new CreateSalesReturnCommand(saleId, request.CustomerId, userId, request.Note),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Returns a paginated list of returns for a sale.
    /// </summary>
    [HttpGet("{saleId:int}/returns")]
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

    /// <summary>
    /// Returns a single sales return by ID with all details.
    /// </summary>
    [HttpGet("{saleId:int}/returns/{returnId:int}")]
    [ProducesResponseType(typeof(SalesReturnDetailsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetReturnById(
        int returnId,
        [FromServices] IGetSalesReturnByIdService getSalesReturnByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getSalesReturnByIdService.ExecuteAsync(new GetSalesReturnByIdQuery(returnId), cancellationToken);
        return MapServiceResult(result);
    }

    /// <summary>
    /// Updates a sales return (note only).
    /// </summary>
    [HttpPut("{saleId:int}/returns/{returnId:int}")]
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

    /// <summary>
    /// Deletes a sales return (soft delete).
    /// </summary>
    [HttpDelete("{saleId:int}/returns/{returnId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteReturn(
        int returnId,
        [FromServices] IDeleteSalesReturnService deleteSalesReturnService,
        CancellationToken cancellationToken)
    {
        var result = await deleteSalesReturnService.ExecuteAsync(returnId, cancellationToken);
        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Sales return deleted" });
    }

    /// <summary>
    /// Adds an item to an existing sales return.
    /// </summary>
    [HttpPost("{saleId:int}/returns/{returnId:int}/items")]
    [ProducesResponseType(typeof(SalesReturnItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddReturnItem(
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

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Updates the quantity of an existing sales return item.
    /// </summary>
    [HttpPut("{saleId:int}/returns/{returnId:int}/items/{itemId:int}")]
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

    /// <summary>
    /// Removes an item from a sales return.
    /// </summary>
    [HttpDelete("{saleId:int}/returns/{returnId:int}/items/{itemId:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
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

        return Ok(new { message = "Item removed" });
    }

    /// <summary>
    /// Completes a sales return (processes stock movements and marks as completed).
    /// </summary>
    [HttpPost("{saleId:int}/returns/{returnId:int}/complete")]
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

    /// <summary>
    /// Cancels a sales return (only from draft state).
    /// </summary>
    [HttpPost("{saleId:int}/returns/{returnId:int}/cancel")]
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

    /// <summary>
    /// Returns the balance remaining on a sales return (total minus payments).
    /// </summary>
    [HttpGet("{saleId:int}/returns/{returnId:int}/balance")]
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

    private int? TryGetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }
}
