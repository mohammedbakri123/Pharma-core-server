using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Requests;
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

    Task<IEnumerable<Payment>> ListAsync(CancellationToken cancellationToken = default);

    Task<PagedResult<Payment>> ListPagedAsync(ListPaymentsQuery query,CancellationToken cancellationToken = default);

    Task<Payment?> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<Payment> Payments, decimal Total)> GetByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);

    Task<IEnumerable<Payment>> GetByReferencesAsync(
        PaymentReferenceType referenceType,
        IEnumerable<int> referenceIds,
        CancellationToken cancellationToken = default);

    Task<bool> SoftDeleteByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);

    Task<decimal> GetTotalRefundedBySaleIdAsync(int saleId, CancellationToken cancellationToken = default);
}
