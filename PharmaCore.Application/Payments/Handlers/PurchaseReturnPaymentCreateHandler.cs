using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Application.Payments.Services;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Handlers;

internal sealed class PurchaseReturnPaymentCreateHandler(
    IPurchaseReturnRepository purchaseReturnRepository,
    IPurchaseRepository purchaseRepository,
    IPaymentRepository paymentRepository) : IPaymentCreateHandler
{
    public PaymentReferenceType ReferenceType => PaymentReferenceType.PURCHASE_RETURN;

    public async Task<ServiceResult<PaymentDto>?> ValidateAsync(
        CreatePaymentCommand command, decimal alreadyPaid, CancellationToken cancellationToken)
    {
        var purchaseReturn = await purchaseReturnRepository.GetByIdAsync(command.ReferenceId, cancellationToken);
        if (purchaseReturn is null)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, $"Purchase return with ID {command.ReferenceId} was not found.");

        if (alreadyPaid + command.Amount > purchaseReturn.TotalAmount)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Payment amount {command.Amount} exceeds remaining amount of {purchaseReturn.TotalAmount - alreadyPaid} for PURCHASE_RETURN:{command.ReferenceId}.");

        var purchase = await purchaseRepository.GetByIdAsync(purchaseReturn.PurchaseId ?? 0, cancellationToken);
        if (purchase is null)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, "Original purchase for this return was not found.");

        var totalPaidOnPurchase = await paymentRepository.GetTotalAmountByReferenceAsync(
            PaymentReferenceType.PURCHASE, purchase.PurchaseId, cancellationToken);

        var maxRefund = PaymentCalculations.ComputePurchaseReturnMaxRefund(purchase.TotalAmount, purchaseReturn.TotalAmount, totalPaidOnPurchase);
        if (alreadyPaid + command.Amount > maxRefund)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Cannot refund {command.Amount}. Maximum refundable amount is {maxRefund} — you have only overpaid by that amount for the goods kept from this purchase.");

        return null;
    }
}
