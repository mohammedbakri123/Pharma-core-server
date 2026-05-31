using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PharmaCore.API.Contracts.Expenses;
using PharmaCore.Application.Expenses.Dtos;
using PharmaCore.Application.Expenses.Interfaces;
using PharmaCore.Application.Expenses.Requests;

namespace PharmaCore.API.Controllers;

/// <summary>
/// Manages expenses and associated payments.
/// </summary>
[Route("expenses")]
[Authorize]
[Tags("Expenses")]
public class ExpensesController : ApiControllerBase
{
    /// <summary>
    /// Returns a paginated list of expenses, optionally filtered by date range.
    /// </summary>
    /// <param name="page">Page number (default 1).</param>
    /// <param name="limit">Items per page (default 20).</param>
    /// <param name="from">Optional start date filter.</param>
    /// <param name="to">Optional end date filter.</param>
    /// <param name="listExpensesService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Paginated list of expenses.</response>
    /// <response code="400">Validation error.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromServices] IListExpensesService listExpensesService = null!,
        CancellationToken cancellationToken = default)
    {
        page = page <= 0 ? 1 : page;
        limit = limit <= 0 ? 20 : limit;

        var result = await listExpensesService.ExecuteAsync(
            new ListExpensesQuery(page, limit, from, to), cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new
        {
            expenses = result.Data!.Items,
            pagination = new
            {
                total = result.Data.Total,
                page = result.Data.Page,
                limit = result.Data.Limit
            }
        });
    }

    /// <summary>
    /// Creates a new expense and an associated payment OUT.
    /// </summary>
    /// <param name="request">The expense data to create.</param>
    /// <param name="createExpenseService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="201">The created expense.</response>
    /// <response code="400">Validation error (e.g. amount must be greater than zero).</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ExpenseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateExpenseRequest request,
        [FromServices] ICreateExpenseService createExpenseService,
        CancellationToken cancellationToken)
    {
        int? userId = TryGetUserId();

        var result = await createExpenseService.ExecuteAsync(
            new CreateExpenseCommand(userId, request.Amount, request.Description),
            cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    /// <summary>
    /// Soft-deletes an expense and its associated payment.
    /// </summary>
    /// <param name="id">The expense ID.</param>
    /// <param name="deleteExpenseService">Injected service.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <response code="200">Confirmation message.</response>
    /// <response code="404">Expense not found.</response>
    /// <response code="401">Unauthorized — missing or invalid JWT.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        int id,
        [FromServices] IDeleteExpenseService deleteExpenseService,
        CancellationToken cancellationToken)
    {
        var result = await deleteExpenseService.ExecuteAsync(id, cancellationToken);

        if (!result.Success)
        {
            return MapServiceResult(result);
        }

        return Ok(new { message = "Expense deleted successfully" });
    }

    private int? TryGetUserId()
    {
        return int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId) ? userId : null;
    }
}
