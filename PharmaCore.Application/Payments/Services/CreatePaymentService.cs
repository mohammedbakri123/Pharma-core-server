using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class CreatePaymentService : ICreatePaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly IPaymentQueryRepository _paymentQueryRepository;
    private readonly ILogger<CreatePaymentService> _logger;

    public CreatePaymentService(
        IPaymentRepository paymentRepository,
        IPaymentQueryRepository paymentQueryRepository,
        ILogger<CreatePaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _paymentQueryRepository = paymentQueryRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PaymentDto>> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.ReferenceId <= 0)
                return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Reference ID must be greater than zero.");

            if (command.Amount <= 0)
                return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Amount must be greater than zero.");

            var paymentId = await _paymentRepository.CreateAsync(
                command.Type,
                command.ReferenceType,
                command.ReferenceId,
                command.Method,
                command.Amount,
                command.Description,
                command.UserId,
                cancellationToken);

            var payment = await _paymentQueryRepository.GetByIdAsync(paymentId, cancellationToken);
            if (payment is null)
                return ServiceResult<PaymentDto>.Fail(ServiceErrorType.ServerError, "Payment was created but could not be retrieved.");

            return ServiceResult<PaymentDto>.Ok(payment);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating payment for reference {ReferenceType}:{ReferenceId}", command.ReferenceType, command.ReferenceId);
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.ServerError, $"Error creating payment: {e.Message}");
        }
    }
}
