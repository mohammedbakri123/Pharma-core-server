using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Expenses.Dtos;
using PharmaCore.Application.Expenses.Interfaces;
using PharmaCore.Application.Expenses.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Services;

public class CreateExpenseService(
    IExpenseRepository expenseRepository,
    IPaymentRepository paymentRepository,
    ILogger<CreateExpenseService> logger)
    : ICreateExpenseService
{
    public async Task<ServiceResult<ExpenseDto>> ExecuteAsync(CreateExpenseCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var expense = Expense.Create(command.UserId, command.Amount, command.Description);
            var created = await expenseRepository.AddAsync(expense, cancellationToken);

            var payment = Payment.Create(
                PaymentType.OUTGOING,
                PaymentReferenceType.EXPENSE,
                created.ExpenseId,
                null,
                command.UserId,
                command.Amount,
                command.Description);

            await paymentRepository.AddAsync(payment, cancellationToken);

            logger.LogInformation("Expense {ExpenseId} created with payment OUT", created.ExpenseId);

            return ServiceResult<ExpenseDto>.Ok(
                new ExpenseDto(created.ExpenseId, created.UserId, created.Amount, created.Description, created.CreatedAt));
        }
        catch (ArgumentException e)
        {
            return ServiceResult<ExpenseDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating expense");
            return ServiceResult<ExpenseDto>.Fail(ServiceErrorType.ServerError, $"Error creating expense: {e.Message}");
        }
    }
}
