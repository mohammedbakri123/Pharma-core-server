using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Expenses.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Services;

public class RestoreDeletedExpenseService(IExpenseRepository expenseRepository, ILogger<RestoreDeletedExpenseService> logger) : IRestoreDeletedExpenseService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await expenseRepository.RestoreDeletedAsync(id, cancellationToken);
            
           if(result) 
               logger.LogInformation("Expense {ExpenseId} restored successfully",id);


            return ServiceResult<bool>.Ok( result);

        }
        catch (Exception e)
        {
            logger.LogError(e, "Error restoring expense");
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error restoring expense: {e.Message}");

        }
    }

}