using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<int> CreateAsync(
        PaymentType type,
        PaymentReferenceType referenceType,
        int referenceId,
        PaymentMethod? method,
        decimal amount,
        string? description,
        int? userId,
        CancellationToken cancellationToken = default)
    {
        var payment = new Models.Payment
        {
            Type = (short)type,
            ReferenceType = (short)referenceType,
            ReferenceId = referenceId,
            Method = method.HasValue ? (short)method.Value : null,
            UserId = userId,
            Amount = amount,
            Description = description,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return payment.PaymentId;
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
}
