using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Expenses.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Services;

public class DeleteExpenseService(
    IExpenseRepository expenseRepository,
    IPaymentRepository paymentRepository,
    ILogger<DeleteExpenseService> logger)
    : IDeleteExpenseService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var expense = await expenseRepository.GetByIdAsync(expenseId, cancellationToken);

            if (expense is null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Expense with ID {expenseId} not found.");
            }

            var deleted = await expenseRepository.SoftDeleteAsync(expenseId, cancellationToken);

            if (!deleted)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete expense.");
            }

            await paymentRepository.SoftDeleteByReferenceAsync(
                PaymentReferenceType.EXPENSE, expenseId, cancellationToken);

            logger.LogInformation("Expense {ExpenseId} and associated payment soft-deleted", expenseId);

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting expense {ExpenseId}", expenseId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error deleting expense: {e.Message}");
        }
    }
}
