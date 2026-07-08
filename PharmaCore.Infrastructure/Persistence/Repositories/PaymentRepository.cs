using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Requests;
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

    public async Task<PagedResult<Payment>> ListPagedAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = dbContext.Payments
            .AsNoTracking().Include(s => s.User)
            .Where(p => p.IsDeleted != true);
            
        // var filtered = payments.AsEnumerable();

        
        if (query.Type.HasValue)
            filtered = filtered.Where(p => p.Type == (short)query.Type.Value);

        if (query.Method.HasValue)
            filtered = filtered.Where(p => p.Method == (short)query.Method.Value);

        if (query.ReferenceType.HasValue)
            filtered = filtered.Where(p => p.ReferenceType == (short)query.ReferenceType.Value);

        if (query.From.HasValue)
            filtered = filtered.Where(p => p.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            filtered = filtered.Where(p => p.CreatedAt <= query.To.Value);
        
        var total = await filtered.CountAsync(cancellationToken);

        var models = await filtered
            .OrderByDescending(s => s.CreatedAt)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<Payment>(
            models.Select(Map).ToList(),
            total,
            query.Page,
            query.Limit);
        
        
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

    public async Task<bool> SoftDeleteByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .ToListAsync(cancellationToken);

        if (models.Count == 0)
            return false;

        foreach (var model in models)
        {
            model.IsDeleted = true;
            model.DeletedAt = DateTimeHelper.GetCurrentTimestamp();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<decimal> GetTotalRefundedBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var returnIds = await dbContext.SalesReturns
            .AsNoTracking()
            .Where(r => r.SaleId == saleId && r.Status == 2 && r.IsDeleted != true)
            .Select(r => r.SalesReturnId)
            .ToListAsync(cancellationToken);

        if (returnIds.Count == 0)
            return 0m;

        return await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)PaymentReferenceType.SALES_RETURN
                && returnIds.Contains(p.ReferenceId)
                && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    public async Task<decimal> GetTotalRefundedByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var returnIds = await dbContext.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.PurchaseId == purchaseId && r.Status == 2 && r.IsDeleted != true)
            .Select(r => r.PurchaseReturnId)
            .ToListAsync(cancellationToken);

        if (returnIds.Count == 0)
            return 0m;

        return await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)PaymentReferenceType.PURCHASE_RETURN
                        && returnIds.Contains(p.ReferenceId)
                        && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    private static Payment Map(Models.Payment model)
    {
        return Payment.Rehydrate(
            model.PaymentId,
            (PaymentType)model.Type,
            (PaymentReferenceType)model.ReferenceType,
            model.ReferenceId,
            model.Method.HasValue ? (PaymentMethod?)model.Method.Value : null,
            model.UserId
            ,model.User?.UserName,
            model.Amount,
            model.Description,
            model.CreatedAt,
            model.IsDeleted,
            model.DeletedAt);
    }
}
