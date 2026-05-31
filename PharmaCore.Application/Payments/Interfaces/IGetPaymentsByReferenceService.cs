using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Interfaces;

public interface IGetPaymentsByReferenceService
{
    Task<ServiceResult<PaymentsByReferenceDto>> ExecuteAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default);
}
