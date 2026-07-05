using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Handlers;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class CreatePaymentService(
    IPaymentRepository paymentRepository,
    IEnumerable<IPaymentCreateHandler> handlers,
    ILogger<CreatePaymentService> logger)
    : ICreatePaymentService
{
    public async Task<ServiceResult<PaymentDto>> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var handler = handlers.FirstOrDefault(h => h.ReferenceType == command.ReferenceType);
            if (handler is null)
                return ServiceResult<PaymentDto>.Fail(
                    ServiceErrorType.Validation,
                    $"Unknown reference type: {command.ReferenceType}.");

            var alreadyPaid = await paymentRepository.GetTotalAmountByReferenceAsync(
                command.ReferenceType, command.ReferenceId, cancellationToken);

            var error = await handler.ValidateAsync(command, alreadyPaid, cancellationToken);
            if (error is not null)
                return error;

            var payment = Payment.Create(
                Payment.DeriveType(command.ReferenceType),
                command.ReferenceType,
                command.ReferenceId,
                command.Method,
                command.UserId,
                null,
                command.Amount,
                command.Description);

            var created = await paymentRepository.AddAsync(payment, cancellationToken);

            return ServiceResult<PaymentDto>.Ok(PaymentMappings.MapToDto(created));
        }
        catch (ArgumentException e)
        {
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating payment for reference {ReferenceType}:{ReferenceId}", command.ReferenceType, command.ReferenceId);
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.ServerError, $"Error creating payment: {e.Message}");
        }
    }
}
