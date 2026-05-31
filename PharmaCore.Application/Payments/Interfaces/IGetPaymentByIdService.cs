using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Interfaces;

public interface IGetPaymentByIdService
{
    Task<ServiceResult<PaymentDto>> ExecuteAsync(int paymentId, CancellationToken cancellationToken = default);
}
