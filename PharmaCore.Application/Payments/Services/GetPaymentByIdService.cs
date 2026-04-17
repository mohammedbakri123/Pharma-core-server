using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class GetPaymentByIdService : IGetPaymentByIdService
{
    private readonly IPaymentQueryRepository _paymentQueryRepository;
    private readonly ILogger<GetPaymentByIdService> _logger;

    public GetPaymentByIdService(IPaymentQueryRepository paymentQueryRepository, ILogger<GetPaymentByIdService> logger)
    {
        _paymentQueryRepository = paymentQueryRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PaymentDto>> ExecuteAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentQueryRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment is null)
                return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, "Payment not found.");

            return ServiceResult<PaymentDto>.Ok(payment);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting payment {PaymentId}", paymentId);
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.ServerError, $"Error getting payment: {e.Message}");
        }
    }
}
