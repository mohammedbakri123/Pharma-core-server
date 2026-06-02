using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Interfaces;

public interface IRestoreDeletedExpenseService
{
    Task<ServiceResult<bool>> ExecuteAsync(int id,CancellationToken cancellationToken = default);

}