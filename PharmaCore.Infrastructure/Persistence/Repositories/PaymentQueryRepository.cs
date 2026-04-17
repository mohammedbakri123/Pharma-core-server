using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class PaymentQueryRepository : IPaymentQueryRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentQueryRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
}
