using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class GetPaymentByIdService : IGetPaymentByIdService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<GetPaymentByIdService> _logger;

    public GetPaymentByIdService(IPaymentRepository paymentRepository, ILogger<GetPaymentByIdService> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PaymentDto>> ExecuteAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        try
        {
            var payment = await _paymentRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment is null)
                return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, "Payment not found.");

            var dto = new PaymentDto(
                payment.PaymentId,
                payment.Type,
                payment.ReferenceType,
                payment.ReferenceId,
                payment.Method,
                payment.UserId,
                null,
                payment.Amount,
                payment.Description,
                payment.CreatedAt);

            return ServiceResult<PaymentDto>.Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting payment {PaymentId}", paymentId);
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.ServerError, $"Error getting payment: {e.Message}");
        }
    }
}
