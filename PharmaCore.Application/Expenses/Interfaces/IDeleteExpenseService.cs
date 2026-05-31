using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Interfaces;

public interface IDeleteExpenseService
{
    Task<ServiceResult<bool>> ExecuteAsync(int expenseId, CancellationToken cancellationToken = default);
}
