using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Application.Payments.Services;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Handlers;

internal sealed class SalesReturnPaymentCreateHandler(
    ISalesReturnRepository salesReturnRepository,
    ISaleRepository saleRepository,
    IPaymentRepository paymentRepository) : IPaymentCreateHandler
{
    public PaymentReferenceType ReferenceType => PaymentReferenceType.SALES_RETURN;

    public async Task<ServiceResult<PaymentDto>?> ValidateAsync(
        CreatePaymentCommand command, decimal alreadyPaid, CancellationToken cancellationToken)
    {
        var salesReturn = await salesReturnRepository.GetByIdAsync(command.ReferenceId, cancellationToken);
        if (salesReturn is null)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, $"Sales return with ID {command.ReferenceId} was not found.");

        if (salesReturn.Status != SalesReturnStatus.Completed)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Cannot create payment for a draft or canceled sale return.");

        if (alreadyPaid + command.Amount > salesReturn.TotalAmount)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Payment amount {command.Amount} exceeds remaining amount of {salesReturn.TotalAmount - alreadyPaid} for SALES_RETURN:{command.ReferenceId}.");

        var sale = await saleRepository.GetByIdAsync(salesReturn.SaleId, cancellationToken);
        if (sale is null)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, "Original sale for this return was not found.");

        var totalPaidOnSale = await paymentRepository.GetTotalAmountByReferenceAsync(
            PaymentReferenceType.SALE, salesReturn.SaleId, cancellationToken);

        var maxRefund = PaymentCalculations.ComputeSalesReturnMaxRefund(sale.TotalAmount, salesReturn.TotalAmount, totalPaidOnSale);
        if (alreadyPaid + command.Amount > maxRefund)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Cannot refund {command.Amount}. Maximum refundable amount is {maxRefund} — the customer has only overpaid by that amount for the goods they kept.");

        return null;
    }
}
