using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Interfaces;

public interface ICreatePaymentService
{
    Task<ServiceResult<PaymentDto>> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default);
}
