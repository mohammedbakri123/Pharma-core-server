using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Application.Payments.Services;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Handlers;

internal sealed class PurchasePaymentCreateHandler(
    IPurchaseRepository purchaseRepository,
    IPurchaseReturnRepository purchaseReturnRepository) : IPaymentCreateHandler
{
    public PaymentReferenceType ReferenceType => PaymentReferenceType.PURCHASE;

    public async Task<ServiceResult<PaymentDto>?> ValidateAsync(
        CreatePaymentCommand command, decimal alreadyPaid, CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetByIdAsync(command.ReferenceId, cancellationToken);
        if (purchase is null)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, $"Purchase with ID {command.ReferenceId} was not found.");

        if (purchase.Status != PurchaseStatus.Completed)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Cannot create payment for an uncompleted purchase.");

        var returnedAmount = await purchaseReturnRepository.GetTotalAmountByPurchaseIdAsync(command.ReferenceId, cancellationToken);
        var remaining = PaymentCalculations.ComputePurchaseRemaining(purchase.TotalAmount, alreadyPaid, returnedAmount);
        if (command.Amount > remaining)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Payment amount {command.Amount} exceeds remaining amount of {Math.Max(0, remaining)} for PURCHASE:{command.ReferenceId}.");

        return null;
    }
}
