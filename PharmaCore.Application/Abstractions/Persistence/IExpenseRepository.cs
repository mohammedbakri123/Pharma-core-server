using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IExpenseRepository
{
    Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default);

    Task<Expense?> GetByIdAsync(int expenseId, CancellationToken cancellationToken = default);

    Task<PagedResult<Expense>> ListAsync( 
        int page,
        int limit,    DateTime? from,
        DateTime? to,CancellationToken cancellationToken = default);

    Task<decimal> GetTotalAmountAsync(CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteAsync(int expenseId, CancellationToken cancellationToken = default);
    
    Task<PagedResult<Expense>> ListDeletedAsync(
      
        int page,
        int limit,
        CancellationToken cancellationToken = default); 
    
    Task<bool> RestoreDeletedAsync(int expenseId, CancellationToken cancellationToken = default);

}
