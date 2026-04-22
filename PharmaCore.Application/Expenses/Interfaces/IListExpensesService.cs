using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Expenses.Dtos;
using PharmaCore.Application.Expenses.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Interfaces;

public interface IListExpensesService
{
    Task<ServiceResult<PagedResult<ExpenseDto>>> ExecuteAsync(ListExpensesQuery query, CancellationToken cancellationToken = default);
}
