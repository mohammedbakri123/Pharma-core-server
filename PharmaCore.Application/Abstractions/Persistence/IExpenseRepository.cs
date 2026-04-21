using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IExpenseRepository
{
    Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default);

    Task<Expense?> GetByIdAsync(int expenseId, CancellationToken cancellationToken = default);

    Task<IEnumerable<Expense>> ListAsync(CancellationToken cancellationToken = default);

    Task<decimal> GetTotalAmountAsync(CancellationToken cancellationToken = default);
}