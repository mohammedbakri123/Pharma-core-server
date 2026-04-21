using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Infrastructure.Utilities;
using ExpenseModel = PharmaCore.Infrastructure.Models.Expense;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class ExpenseRepository(ApplicationDbContext dbContext) : IExpenseRepository
{
    public async Task<Expense> AddAsync(Expense expense, CancellationToken cancellationToken = default)
    {
        var model = new ExpenseModel
        {
            UserId = expense.UserId,
            Amount = expense.Amount,
            Description = expense.Description,
            CreatedAt = DateTimeHelper.GetCurrentTimestamp(),
            IsDeleted = false
        };

        dbContext.Expenses.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<Expense?> GetByIdAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Expenses
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<IEnumerable<Expense>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.IsDeleted != true)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<decimal> GetTotalAmountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.IsDeleted != true)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;
    }

    private static Expense Map(ExpenseModel model)
    {
        return Expense.Rehydrate(
            model.ExpenseId,
            model.UserId,
            model.Amount ?? 0m,
            model.Description,
            model.CreatedAt,
            model.IsDeleted,
            model.DeletedAt);
    }
}