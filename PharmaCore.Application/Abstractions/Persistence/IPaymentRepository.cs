using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task<int> CreateAsync(
        PaymentType type,
        PaymentReferenceType referenceType,
        int referenceId,
        PaymentMethod? method,
        decimal amount,
        string? description,
        int? userId,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalAmountByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);
}
