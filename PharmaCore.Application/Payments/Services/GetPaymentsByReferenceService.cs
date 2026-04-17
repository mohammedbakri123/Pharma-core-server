using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class GetPaymentsByReferenceService : IGetPaymentsByReferenceService
{
    private readonly IPaymentQueryRepository _paymentQueryRepository;
    private readonly ILogger<GetPaymentsByReferenceService> _logger;

    public GetPaymentsByReferenceService(
        IPaymentQueryRepository paymentQueryRepository,
        ILogger<GetPaymentsByReferenceService> logger)
    {
        _paymentQueryRepository = paymentQueryRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PaymentsByReferenceDto>> ExecuteAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (referenceId <= 0)
                return ServiceResult<PaymentsByReferenceDto>.Fail(ServiceErrorType.Validation, "Reference ID must be greater than zero.");

            var payments = await _paymentQueryRepository.GetByReferenceAsync(referenceType, referenceId, cancellationToken);
            return ServiceResult<PaymentsByReferenceDto>.Ok(payments);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting payments for {ReferenceType}:{ReferenceId}", referenceType, referenceId);
            return ServiceResult<PaymentsByReferenceDto>.Fail(ServiceErrorType.ServerError, $"Error getting payments: {e.Message}");
        }
    }
}
