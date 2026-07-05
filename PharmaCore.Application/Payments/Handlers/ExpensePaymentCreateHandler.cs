using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Handlers;

internal sealed class ExpensePaymentCreateHandler(IExpenseRepository expenseRepository) : IPaymentCreateHandler
{
    public PaymentReferenceType ReferenceType => PaymentReferenceType.EXPENSE;

    public async Task<ServiceResult<PaymentDto>?> ValidateAsync(
        CreatePaymentCommand command, decimal alreadyPaid, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByIdAsync(command.ReferenceId, cancellationToken);
        if (expense is null)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, $"Expense with ID {command.ReferenceId} was not found.");

        if (alreadyPaid + command.Amount > expense.Amount)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Payment amount {command.Amount} exceeds remaining amount of {expense.Amount - alreadyPaid} for EXPENSE:{command.ReferenceId}.");

        return null;
    }
}
