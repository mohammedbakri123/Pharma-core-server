using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default);

    Task<decimal> GetTotalAmountByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);

    Task<PagedResult<PaymentDto>> ListAsync(
        int page,
        int limit,
        PaymentType? type,
        PaymentMethod? method,
        PaymentReferenceType? referenceType,
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);

    Task<PaymentDto?> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default);

    Task<PaymentsByReferenceDto> GetByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);
}
