using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class CreatePaymentService : ICreatePaymentService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<CreatePaymentService> _logger;

    public CreatePaymentService(
        IPaymentRepository paymentRepository,
        ILogger<CreatePaymentService> logger)
    {
        _paymentRepository = paymentRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PaymentDto>> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var referenceExists = await _paymentRepository.ExistsAsync(
                command.ReferenceType,
                command.ReferenceId,
                cancellationToken);

            if (!referenceExists)
                return ServiceResult<PaymentDto>.Fail(
                    ServiceErrorType.NotFound,
                    $"Referenced {command.ReferenceType} record was not found.");

            var payment = Payment.Create(
                command.Type,
                command.ReferenceType,
                command.ReferenceId,
                command.Method,
                command.UserId,
                command.Amount,
                command.Description);

            var createdPayment = await _paymentRepository.AddAsync(payment, cancellationToken);

            var paymentDto = await _paymentRepository.GetByIdAsync(createdPayment.PaymentId, cancellationToken);
            if (paymentDto is null)
                return ServiceResult<PaymentDto>.Fail(ServiceErrorType.ServerError, "Payment was created but could not be retrieved.");

            return ServiceResult<PaymentDto>.Ok(paymentDto);
        }
        catch (ArgumentException e)
        {
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating payment for reference {ReferenceType}:{ReferenceId}", command.ReferenceType, command.ReferenceId);
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.ServerError, $"Error creating payment: {e.Message}");
        }
    }
}
