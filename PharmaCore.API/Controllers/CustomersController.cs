using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.API.Contracts.Customers;
using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages customers and their financial operations.
/// </summary>
[Route("customers")]
[Authorize]
[Tags("Customers")]
public class CustomersController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of customers, optionally filtered by search term.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromServices] IListCustomersService listCustomersService = null!,
        CancellationToken cancellationToken = default)
    {
        //TODO: business should be handled at application layer
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listCustomersService.ExecuteAsync(
            new ListCustomersQuery(page, limit, search), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            customers = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Returns a single customer by ID.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetCustomerByIdService getCustomerByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getCustomerByIdService.ExecuteAsync(
            new GetCustomerByIdQuery(id), cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Creates a new customer.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCustomerRequest request,
        [FromServices] ICreateCustomerService createCustomerService,
        CancellationToken cancellationToken)
    {
        var result = await createCustomerService.ExecuteAsync(
            new CreateCustomerCommand(request.Name, request.PhoneNumber, request.Address, request.Note),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(201, result.Data);
    }

    /// <summary>
    /// Returns a paginated list of soft-deleted customers.
    /// </summary>
    [HttpGet("deleted")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListDeleted(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromServices] IListDeletedCustomersService listDeletedCustomersService = null!,
        CancellationToken cancellationToken = default)
    {
        //TODO: business should be handled at application layer

        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listDeletedCustomersService.ExecuteAsync(
            new ListDeletedCustomersQuery(page, limit, search), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            customers = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Restores a soft-deleted customer.
    /// </summary>
    [HttpPost("{id:int}/restore")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Restore(
        int id,
        [FromServices] IRestoreCustomerService restoreCustomerService,
        CancellationToken cancellationToken)
    {
        var result = await restoreCustomerService.ExecuteAsync(
            new RestoreCustomerCommand(id), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Customer restored successfully" });
    }

    /// <summary>
    /// Updates an existing customer.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(CustomerDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateCustomerRequest request,
        [FromServices] IUpdateCustomerService updateCustomerService,
        CancellationToken cancellationToken)
    {
        var result = await updateCustomerService.ExecuteAsync(
            new UpdateCustomerCommand(id, request.Name, request.PhoneNumber, request.Address, request.Note),
            cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Soft-deletes a customer.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteCustomerService deleteCustomerService,
        CancellationToken cancellationToken)
    {
        var result = await deleteCustomerService.ExecuteAsync(
            new DeleteCustomerCommand(id), cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Customer deleted successfully" });
    }

    /// <summary>
    /// Returns customer invoices (sales).
    /// </summary>
    [HttpGet("{id:int}/sales")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Sales(
        int id,
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] short? status = null,
        [FromServices] IListSalesService listSalesService = null!,
        CancellationToken cancellationToken = default)
    {
        //TODO: business should be handled at application layer

        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listSalesService.ExecuteAsync(
            new ListSalesQuery(page, limit, id, null, (SaleStatus?)status, null, null),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            customerId = id,
            sales = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Returns total debt for a customer (sales - payments - returns).
    /// </summary>
    [HttpGet("{id:int}/debt")]
    [ProducesResponseType(typeof(SalesSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Debt(
        int id,
        [FromServices] IGetSalesSummaryService getSalesSummaryService,
        CancellationToken cancellationToken)
    {
        var result = await getSalesSummaryService.ExecuteAsync(id, cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Returns list of unpaid invoices for a customer.
    /// </summary>
    [HttpGet("{id:int}/unpaid-sales")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnpaidSales(
        int id,
        [FromServices] IGetUnpaidSalesService getUnpaidSalesService,
        CancellationToken cancellationToken)
    {
        var result = await getUnpaidSalesService.ExecuteAsync(id, cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new
        {
            customerId = id,
            unpaidSales = result.Data
        });
    }

    /// <summary>
    /// Returns full transaction history (like a bank statement).
    /// </summary>
    [HttpGet("{id:int}/statement")]
    [ProducesResponseType(typeof(SalesStatementDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Statement(
        int id,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromServices] IGetSalesStatementService getSalesStatementService = null!,
        CancellationToken cancellationToken = default)
    {
        var result = await getSalesStatementService.ExecuteAsync(id, from, to, cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Pays customer debt — auto-distributes to oldest unpaid sales first.
    /// </summary>
    [HttpPost("{id:int}/pay")]
    [ProducesResponseType(typeof(PayCustomerDebtResult), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Pay(
        int id,
        [FromBody] PayCustomerDebtRequest request,
        [FromServices] IPayCustomerDebtService payCustomerDebtService,
        CancellationToken cancellationToken)
    {
        var result = await payCustomerDebtService.ExecuteAsync(
            new PayCustomerDebtCommand(id, request.Amount, request.Method, request.Description),
            cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return StatusCode(201, result.Data);
    }

    /// <summary>
    /// Permanently deletes a customer from the database.
    /// </summary>
    [HttpDelete("{id:int}/hard")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> HardDelete(
        int id,
        [FromServices] IHardDeleteCustomerService hardDeleteCustomerService,
        CancellationToken cancellationToken)
    {
        var result = await hardDeleteCustomerService.ExecuteAsync(id, cancellationToken);

        if (!result.Success)
            return MapServiceResult(result);

        return Ok(new { message = "Customer permanently deleted" });
    }
}
