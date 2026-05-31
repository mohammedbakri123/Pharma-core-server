using PharmaCore.Application.Expenses.Dtos;
using PharmaCore.Application.Expenses.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Expenses.Interfaces;

public interface ICreateExpenseService
{
    Task<ServiceResult<ExpenseDto>> ExecuteAsync(CreateExpenseCommand command, CancellationToken cancellationToken = default);
}
