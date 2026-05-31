using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Sales;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
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
        var result = await createSaleService.ExecuteAsync(new CreateSaleCommand(userId, request.CustomerId, request.Note, request.Discount), cancellationToken);
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
    [ProducesResponseType(typeof(SaleItemDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddItem(
        int id,
        [FromBody] AddSaleItemRequest request,
        [FromServices] IAddSaleItemService addSaleItemService,
        CancellationToken cancellationToken)
    {
        var result = await addSaleItemService.ExecuteAsync(new AddSaleItemCommand(id, request.MedicineId, request.Quantity, request.UnitPrice), cancellationToken);
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
    /// Completes a sale by processing payments and updating inventory. The completing user is automatically assigned.
    /// </summary>
    /// <param name="id">Sale ID.</param>
    /// <param name="request">Complete sale request body.</param>
    /// <param name="completeSaleService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Sale completed successfully.</response>
    /// <response code="400">Validation error or insufficient payment.</response>
    /// <response code="404">Sale not found.</response>
    [HttpPost("{id:int}/complete")]
    [ProducesResponseType(typeof(CompleteSaleResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Complete(
        int id,
        [FromBody] CompleteSaleRequest request,
        [FromServices] ICompleteSaleService completeSaleService,
        CancellationToken cancellationToken)
    {
        int? userId = TryGetUserId();
        var payments = (request.Payments ?? Array.Empty<SalePaymentRequest>())
            .Select(payment => new SalePaymentInputDto(payment.Amount, payment.Method, payment.Description))
            .ToList();

        var result = await completeSaleService.ExecuteAsync(new CompleteSaleCommand(id, userId, payments), cancellationToken);
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

    private int? TryGetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }
}
