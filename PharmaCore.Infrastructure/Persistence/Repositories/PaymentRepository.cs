using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class PaymentRepository(ApplicationDbContext dbContext) : IPaymentRepository
{
    public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var model = new Models.Payment
        {
            Type = (short)payment.Type,
            ReferenceType = (short)payment.ReferenceType,
            ReferenceId = payment.ReferenceId,
            Method = payment.Method.HasValue ? (short)payment.Method.Value : null,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Description = payment.Description,
            CreatedAt = DateTimeHelper.GetCurrentTimestamp(),
        };

        dbContext.Payments.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<decimal> GetTotalAmountByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        return  await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

       
    }

    
    public async Task<IEnumerable<Payment>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.IsDeleted != true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    
    public async Task<Payment?> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    
    public async Task<(IReadOnlyList<Payment> Payments, decimal Total)> GetByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var payments = models.Select(Map).ToList();
        return (payments, payments.Sum(p => p.Amount));
    }

    
    public async Task<IEnumerable<Payment>> GetByReferencesAsync(
        PaymentReferenceType referenceType,
        IEnumerable<int> referenceIds,
        CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)referenceType && referenceIds.Contains(p.ReferenceId) && p.IsDeleted != true)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    
    public Task<bool> ExistsAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        return referenceType switch
        {
            PaymentReferenceType.SALE => dbContext.Sales
                .AsNoTracking()
                .AnyAsync(s => s.SaleId == referenceId && s.IsDeleted != true, cancellationToken),
            PaymentReferenceType.PURCHASE => dbContext.Purchases
                .AsNoTracking()
                .AnyAsync(p => p.PurchaseId == referenceId && p.IsDeleted != true, cancellationToken),
            PaymentReferenceType.EXPENSE => dbContext.Expenses
                .AsNoTracking()
                .AnyAsync((Models.Expense e) => e.ExpenseId == referenceId && e.IsDeleted != true, cancellationToken),
            PaymentReferenceType.SALES_RETURN => dbContext.SalesReturns
                .AsNoTracking()
                .AnyAsync(r => r.SalesReturnId == referenceId && r.IsDeleted != true, cancellationToken),
            _ => Task.FromResult(false)
        };
    }

    private static Payment Map(Models.Payment model)
    {
        return Payment.Rehydrate(
            model.PaymentId,
            (PaymentType)model.Type,
            (PaymentReferenceType)model.ReferenceType,
            model.ReferenceId,
            model.Method.HasValue ? (PaymentMethod?)model.Method.Value : null,
            model.UserId,
            model.Amount,
            model.Description,
            model.CreatedAt,
            model.IsDeleted,
            model.DeletedAt);
    }
}
