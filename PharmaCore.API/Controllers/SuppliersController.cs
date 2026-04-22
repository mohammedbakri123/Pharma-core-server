using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Suppliers;
using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Interfaces;
using PharmaCore.Application.Suppliers.Requests;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages suppliers.
/// </summary>
[Route("suppliers")]
[Authorize]
[Tags("Suppliers")]
public class SuppliersController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of suppliers, optionally filtered by search term.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="search">Optional search keyword to filter by name or phone.</param>
    /// <param name="listSuppliersService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of suppliers.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? search = null,
        [FromServices] IListSuppliersService listSuppliersService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listSuppliersService.ExecuteAsync(
            new ListSuppliersQuery(page, limit, search), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new
        {
            suppliers = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Returns a single supplier by its ID.
    /// </summary>
    /// <param name="id">The supplier ID.</param>
    /// <param name="getSupplierByIdService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The requested supplier.</response>
    /// <response code="404">Supplier not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] IGetSupplierByIdService getSupplierByIdService,
        CancellationToken cancellationToken)
    {
        var result = await getSupplierByIdService.ExecuteAsync(
            new GetSupplierByIdQuery(id), cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Creates a new supplier.
    /// </summary>
    /// <param name="request">The supplier data to create.</param>
    /// <param name="createSupplierService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The created supplier.</response>
    /// <response code="400">Validation error (e.g. missing name).</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    /// <response code="409">A supplier with the same name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        [FromServices] ICreateSupplierService createSupplierService,
        CancellationToken cancellationToken)
    {
        var result = await createSupplierService.ExecuteAsync(
            new CreateSupplierCommand(request.Name, request.PhoneNumber, request.Address),
            cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Updates an existing supplier.
    /// </summary>
    /// <param name="id">The supplier ID.</param>
    /// <param name="request">The updated supplier data.</param>
    /// <param name="updateSupplierService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">The updated supplier.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="404">Supplier not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(SupplierDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateSupplierRequest request,
        [FromServices] IUpdateSupplierService updateSupplierService,
        CancellationToken cancellationToken)
    {
        var result = await updateSupplierService.ExecuteAsync(
            new UpdateSupplierCommand(id, request.Name, request.PhoneNumber, request.Address),
            cancellationToken);

        return MapServiceResult(result);
    }

    /// <summary>
    /// Soft-deletes a supplier.
    /// </summary>
    /// <param name="id">The supplier ID.</param>
    /// <param name="deleteSupplierService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Confirmation message.</response>
    /// <response code="404">Supplier not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteSupplierService deleteSupplierService,
        CancellationToken cancellationToken)
    {
        var result = await deleteSupplierService.ExecuteAsync(id, cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new { message = "Supplier deleted successfully" });
    }
}
