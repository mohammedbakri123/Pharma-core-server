using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Expenses.Dtos;
using PharmaCore.Application.Expenses.Interfaces;
using PharmaCore.Application.Expenses.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Services;

public class ListDeletedExpense(IExpenseRepository expenseRepository, ILogger logger)
: IListExpensesService
{
    public async Task<ServiceResult<PagedResult<ExpenseDto>>> ExecuteAsync(ListExpensesQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expenses =
                await expenseRepository.ListDeletedAsync(query.Page, query.Limit, query.From, query.To, cancellationToken);



            return ServiceResult<PagedResult<ExpenseDto>>.Ok(MapToDto(expenses));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting expense list");
            return ServiceResult<PagedResult<ExpenseDto>>.Fail(ServiceErrorType.ServerError,
                $"Error getting expense list: {e.Message}");
        }
    }
    private static PagedResult<ExpenseDto> MapToDto(PagedResult<Expense> result)
    {
        var items = result.Items
            .Select(e => new ExpenseDto(
                e.ExpenseId,
                e.UserId,
                e.Amount,
                e.Description,
                e.CreatedAt
            ))
            .ToList();

        return new PagedResult<ExpenseDto>(items, result.Total, result.Page, result.Limit);
    }
    
}