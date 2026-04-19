using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

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
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Payments.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<decimal> GetTotalAmountByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    public async Task<IEnumerable<PaymentDto>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .ToListAsync(cancellationToken);
        return models.Select(MapProjection().Compile()).ToList();
    }

    public async Task<PagedResult<PaymentDto>> ListAsync(
        int page,
        int limit,
        PaymentType? type,
        PaymentMethod? method,
        PaymentReferenceType? referenceType,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.IsDeleted != true)
            .AsQueryable();

        if (type.HasValue)
            query = query.Where(p => p.Type == (short)type.Value);

        if (method.HasValue)
            query = query.Where(p => p.Method == (short)method.Value);

        if (referenceType.HasValue)
            query = query.Where(p => p.ReferenceType == (short)referenceType.Value);

        if (from.HasValue)
            query = query.Where(p => p.CreatedAt >= from.Value);

        if (to.HasValue)
            query = query.Where(p => p.CreatedAt <= to.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(MapProjection())
            .ToListAsync(cancellationToken);

        return new PagedResult<PaymentDto>(items, total, page, limit);
    }

    public async Task<PaymentDto?> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.PaymentId == paymentId && p.IsDeleted != true)
            .Select(MapProjection())
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PaymentsByReferenceDto> GetByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        var payments = await _dbContext.Payments
            .AsNoTracking()
            .Include(p => p.User)
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .OrderBy(p => p.CreatedAt)
            .Select(MapProjection())
            .ToListAsync(cancellationToken);

        return new PaymentsByReferenceDto(payments, payments.Sum(p => p.Amount));
    }

    public Task<bool> ExistsAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        return referenceType switch
        {
            PaymentReferenceType.SALE => _dbContext.Sales
                .AsNoTracking()
                .AnyAsync(s => s.SaleId == referenceId && s.IsDeleted != true, cancellationToken),
            PaymentReferenceType.PURCHASE => _dbContext.Purchases
                .AsNoTracking()
                .AnyAsync(p => p.PurchaseId == referenceId && p.IsDeleted != true, cancellationToken),
            PaymentReferenceType.EXPENSE => _dbContext.Expenses
                .AsNoTracking()
                .AnyAsync(e => e.ExpenseId == referenceId && e.IsDeleted != true, cancellationToken),
            PaymentReferenceType.SALES_RETURN => _dbContext.SalesReturns
                .AsNoTracking()
                .AnyAsync(r => r.SalesReturnId == referenceId && r.IsDeleted != true, cancellationToken),
            _ => Task.FromResult(false)
        };
    }

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
