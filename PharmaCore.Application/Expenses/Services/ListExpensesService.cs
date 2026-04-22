using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Expenses.Dtos;
using PharmaCore.Application.Expenses.Interfaces;
using PharmaCore.Application.Expenses.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Services;

public class ListExpensesService(IExpenseRepository expenseRepository, ILogger<ListExpensesService> logger)
    : IListExpensesService
{
    public async Task<ServiceResult<PagedResult<ExpenseDto>>> ExecuteAsync(ListExpensesQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expenses = await expenseRepository.ListAsync(cancellationToken);
            var filtered = expenses.AsQueryable();

            if (query.From.HasValue)
            {
                filtered = filtered.Where(e => e.CreatedAt >= query.From.Value);
            }

            if (query.To.HasValue)
            {
                var toDate = query.To.Value.Date.AddDays(1).AddTicks(-1);
                filtered = filtered.Where(e => e.CreatedAt <= toDate);
            }

            var total = filtered.Count();
            var items = filtered
                .OrderByDescending(e => e.CreatedAt)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(e => new ExpenseDto(e.ExpenseId, e.UserId, e.Amount, e.Description, e.CreatedAt))
                .ToList();

            return ServiceResult<PagedResult<ExpenseDto>>.Ok(
                new PagedResult<ExpenseDto>(items, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting expense list");
            return ServiceResult<PagedResult<ExpenseDto>>.Fail(ServiceErrorType.ServerError, $"Error getting expense list: {e.Message}");
        }
    }
}
