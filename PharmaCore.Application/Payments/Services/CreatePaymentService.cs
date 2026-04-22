using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class CreatePaymentService(
    ISaleRepository saleRepository,
    IPurchaseRepository purchaseRepository,
    IExpenseRepository expenseRepository,
    ISalesReturnRepository salesReturnRepository,
    IPaymentRepository paymentRepository,
    ILogger<CreatePaymentService> logger)
    : ICreatePaymentService
{
    public async Task<ServiceResult<PaymentDto>> ExecuteAsync(CreatePaymentCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var referenceExists = await CheckReferenceExistsAsync(
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

            var createdPayment = await paymentRepository.AddAsync(payment, cancellationToken);

            var paymentDto = new PaymentDto(
                createdPayment.PaymentId,
                createdPayment.Type,
                createdPayment.ReferenceType,
                createdPayment.ReferenceId,
                createdPayment.Method,
                createdPayment.UserId,
                null,
                createdPayment.Amount,
                createdPayment.Description,
                createdPayment.CreatedAt);

            return ServiceResult<PaymentDto>.Ok(paymentDto);
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

    private async Task<bool> CheckReferenceExistsAsync(PaymentReferenceType referenceType, int referenceId, CancellationToken cancellationToken)
    {
        return referenceType switch
        {
            PaymentReferenceType.SALE => await saleRepository.GetByIdAsync(referenceId, cancellationToken) is not null,
            PaymentReferenceType.PURCHASE => await purchaseRepository.GetByIdAsync(referenceId, cancellationToken) is not null,
            PaymentReferenceType.EXPENSE => await expenseRepository.GetByIdAsync(referenceId, cancellationToken) is not null,
            PaymentReferenceType.SALES_RETURN => await salesReturnRepository.GetByIdAsync(referenceId, cancellationToken) is not null,
            _ => false
        };
    }
}
