using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
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

    
    //TODO: this should return payment entity
    public async Task<IEnumerable<PaymentDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.IsDeleted != true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
        return models.Select(MapProjection().Compile()).ToList();
    }

    
    //TODO: this should return payment entity
    public async Task<PaymentDto?> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.PaymentId == paymentId && p.IsDeleted != true)
            .Select(MapProjection())
            .FirstOrDefaultAsync(cancellationToken);
    }

    //TODO: this should return payment entity
    public async Task<PaymentsByReferenceDto> GetByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        var payments = await dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .OrderBy(p => p.CreatedAt)
            .Select(MapProjection())
            .ToListAsync(cancellationToken);

        return new PaymentsByReferenceDto(payments, payments.Sum(p => p.Amount));
    }

    //TODO: this should return payment entity
    //TODO: is this ok or we should use (ListAsync) then filter it in application layer
    public async Task<IEnumerable<PaymentDto>> GetByReferencesAsync(
        PaymentReferenceType referenceType,
        IEnumerable<int> referenceIds,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.ReferenceType == (short)referenceType && referenceIds.Contains(p.ReferenceId) && p.IsDeleted != true)
            .OrderBy(p => p.CreatedAt)
            .Select(MapProjection())
            .ToListAsync(cancellationToken);
    }

    
    ///TODO: this function uses the sales context I should fix that by deleting the function,
    /// this function is made to check wither the refrence ,we need to made the paymeny for, exist or not,
    /// we need to move this logic to createPayment service
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
                .AnyAsync(e => e.ExpenseId == referenceId && e.IsDeleted != true, cancellationToken),
            PaymentReferenceType.SALES_RETURN => dbContext.SalesReturns
                .AsNoTracking()
                .AnyAsync(r => r.SalesReturnId == referenceId && r.IsDeleted != true, cancellationToken),
            _ => Task.FromResult(false)
        };
    }

    //TODO: after making functions use the entity instead of dto, this must be deleted
    private static System.Linq.Expressions.Expression<Func<Models.Payment, PaymentDto>> MapProjection()
    {
        return payment => new PaymentDto(
            payment.PaymentId,
            (PaymentType)payment.Type,
            (PaymentReferenceType)payment.ReferenceType,
            payment.ReferenceId,
            payment.Method.HasValue ? (PaymentMethod?)payment.Method.Value : null,
            payment.UserId,
            payment.User != null ? payment.User.UserName : null,
            payment.Amount,
            payment.Description,
            payment.CreatedAt);
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
