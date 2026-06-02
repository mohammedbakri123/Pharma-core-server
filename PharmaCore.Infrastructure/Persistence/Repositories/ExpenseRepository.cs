using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
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

    public async Task<PagedResult<Expense>> ListAsync( 
        int page,
        int limit,    DateTime? from,
        DateTime? to,CancellationToken cancellationToken = default)
    {
        var query = dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.IsDeleted != true);
        
        if (from.HasValue)
        {
            var normalizedFrom = DateTimeHelper.NormalizeTimestamp(from.Value);
            query = query.Where(e => e.CreatedAt >= normalizedFrom);
        }

        if (to.HasValue)
        {
            var normalizedTo = DateTimeHelper.NormalizeTimestamp(to.Value);
            query = query.Where(e => e.CreatedAt <= normalizedTo);
        }

        var total = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<Expense>(
            items.Select(Map).ToList(),
            total,
            page,
            limit);
            
     

     
    }

    public async Task<decimal> GetTotalAmountAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.IsDeleted != true)
            .SumAsync(e => (decimal?)e.Amount, cancellationToken) ?? 0m;
    }

    public async Task<bool> SoftDeleteAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Expenses
            .FirstOrDefaultAsync(e => e.ExpenseId == expenseId && e.IsDeleted != true, cancellationToken);

        if (model is null)
            return false;

        model.IsDeleted = true;
        model.DeletedAt = DateTimeHelper.GetCurrentTimestamp();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
    
    public async Task<PagedResult<Expense>> ListDeletedAsync( 
        int page,
        int limit,CancellationToken cancellationToken = default)
    {
        var query = dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.IsDeleted == true)
            .OrderByDescending(e => e.CreatedAt);
        
        var total = await query.CountAsync(cancellationToken);
        
        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<Expense>(
            items.Select(Map).ToList(),
            total,
            page,
            limit);
            
     

     
    }
    public async Task<bool> RestoreDeletedAsync(int expenseId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE expenses SET is_deleted = FALSE, deleted_at = NULL WHERE expense_id = {expenseId} AND is_deleted IS TRUE",
            cancellationToken);

        return affectedRows > 0;
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