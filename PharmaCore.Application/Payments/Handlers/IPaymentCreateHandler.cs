using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Handlers;

public interface IPaymentCreateHandler
{
    PaymentReferenceType ReferenceType { get; }
    Task<ServiceResult<PaymentDto>?> ValidateAsync(
        CreatePaymentCommand command,
        decimal alreadyPaid,
        CancellationToken cancellationToken);
}
