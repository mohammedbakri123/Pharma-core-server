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
    IPurchaseReturnRepository purchaseReturnRepository,
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
            var reference = await FetchReferenceAsync(command.ReferenceType, command.ReferenceId, cancellationToken);
            if (reference is null)
                return ServiceResult<PaymentDto>.Fail(
                    ServiceErrorType.NotFound,
                    $"Referenced {command.ReferenceType} record was not found.");

            var validationError = await ValidateReferenceAsync(reference, command, cancellationToken);
            if (validationError is not null)
                return validationError;

            var payment = Payment.Create(
                Payment.DeriveType(command.ReferenceType),
                command.ReferenceType,
                command.ReferenceId,
                command.Method,
                command.UserId,
                null,
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

    private abstract record ReferenceInfo(decimal TotalAmount);

    private sealed record SaleReferenceInfo(decimal TotalAmount, SaleStatus Status) : ReferenceInfo(TotalAmount);

    private sealed record PurchaseReferenceInfo(decimal TotalAmount, PurchaseStatus Status) : ReferenceInfo(TotalAmount);

    private sealed record ExpenseReferenceInfo(decimal TotalAmount) : ReferenceInfo(TotalAmount);

    private sealed record SalesReturnReferenceInfo(decimal TotalAmount, SalesReturnStatus Status, int SaleId) : ReferenceInfo(TotalAmount);

    private sealed record PurchaseReturnReferenceInfo(decimal TotalAmount) : ReferenceInfo(TotalAmount);

    private async Task<ReferenceInfo?> FetchReferenceAsync(PaymentReferenceType referenceType, int referenceId, CancellationToken cancellationToken)
    {
        return referenceType switch
        {
            PaymentReferenceType.SALE => await FetchSaleAsync(referenceId, cancellationToken),
            PaymentReferenceType.PURCHASE => await FetchPurchaseAsync(referenceId, cancellationToken),
            PaymentReferenceType.EXPENSE => await FetchExpenseAsync(referenceId, cancellationToken),
            PaymentReferenceType.SALES_RETURN => await FetchSalesReturnAsync(referenceId, cancellationToken),
            PaymentReferenceType.PURCHASE_RETURN => await FetchPurchaseReturnAsync(referenceId, cancellationToken),
            _ => null
        };
    }

    private async Task<ReferenceInfo?> FetchSaleAsync(int referenceId, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(referenceId, cancellationToken);
        return sale is null ? null : new SaleReferenceInfo(sale.TotalAmount, sale.Status);
    }

    private async Task<ReferenceInfo?> FetchPurchaseAsync(int referenceId, CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetByIdAsync(referenceId, cancellationToken);
        return purchase is null ? null : new PurchaseReferenceInfo(purchase.TotalAmount, purchase.Status);
    }

    private async Task<ReferenceInfo?> FetchExpenseAsync(int referenceId, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByIdAsync(referenceId, cancellationToken);
        return expense is null ? null : new ExpenseReferenceInfo(expense.Amount);
    }

    private async Task<ReferenceInfo?> FetchSalesReturnAsync(int referenceId, CancellationToken cancellationToken)
    {
        var salesReturn = await salesReturnRepository.GetByIdAsync(referenceId, cancellationToken);
        return salesReturn is null ? null : new SalesReturnReferenceInfo(salesReturn.TotalAmount, salesReturn.Status, salesReturn.SaleId);
    }

    private async Task<ReferenceInfo?> FetchPurchaseReturnAsync(int referenceId, CancellationToken cancellationToken)
    {
        var purchaseReturn = await purchaseReturnRepository.GetByIdAsync(referenceId, cancellationToken);
        return purchaseReturn is null ? null : new PurchaseReturnReferenceInfo(purchaseReturn.TotalAmount);
    }

    private async Task<ServiceResult<PaymentDto>?> ValidateReferenceAsync(ReferenceInfo reference, CreatePaymentCommand command, CancellationToken cancellationToken)
    {
        var alreadyPaid = await paymentRepository.GetTotalAmountByReferenceAsync(
            command.ReferenceType, command.ReferenceId, cancellationToken);

        return reference switch
        {
            SaleReferenceInfo sale => ValidateSale(sale, command, alreadyPaid),
            PurchaseReferenceInfo purchase => ValidatePurchase(purchase, command, alreadyPaid),
            ExpenseReferenceInfo expense => ValidateExpense(expense, command, alreadyPaid),
            SalesReturnReferenceInfo salesReturn => await ValidateSalesReturnAsync(salesReturn, command, alreadyPaid, cancellationToken),
            PurchaseReturnReferenceInfo purchaseReturn => ValidatePurchaseReturn(purchaseReturn, command, alreadyPaid),
            _ => null
        };
    }

    private static ServiceResult<PaymentDto>? ValidateOverpayment(decimal alreadyPaid, decimal amount, decimal totalAmount, string referenceLabel)
    {
        if (alreadyPaid + amount > totalAmount)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Payment amount {amount} exceeds remaining amount of {totalAmount - alreadyPaid} for {referenceLabel}.");

        return null;
    }

    private static ServiceResult<PaymentDto>? ValidateSale(SaleReferenceInfo reference, CreatePaymentCommand command, decimal alreadyPaid)
    {
        if (reference.Status != SaleStatus.COMPLETED)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation, "Cannot create payment for a draft or canceled sale.");

        return ValidateOverpayment(alreadyPaid, command.Amount, reference.TotalAmount, $"SALE:{command.ReferenceId}");
    }

    private static ServiceResult<PaymentDto>? ValidatePurchase(PurchaseReferenceInfo reference, CreatePaymentCommand command, decimal alreadyPaid)
    {
        if (reference.Status != PurchaseStatus.Completed)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation, "Cannot create payment for a uncompleted purchase.");

        return ValidateOverpayment(alreadyPaid, command.Amount, reference.TotalAmount, $"PURCHASE:{command.ReferenceId}");
    }

    private static ServiceResult<PaymentDto>? ValidateExpense(ExpenseReferenceInfo reference, CreatePaymentCommand command, decimal alreadyPaid)
    {
        return ValidateOverpayment(alreadyPaid, command.Amount, reference.TotalAmount, $"EXPENSE:{command.ReferenceId}");
    }

    private async Task<ServiceResult<PaymentDto>?> ValidateSalesReturnAsync(
        SalesReturnReferenceInfo reference, CreatePaymentCommand command, decimal alreadyPaid, CancellationToken cancellationToken)
    {
        if (reference.Status != SalesReturnStatus.Completed)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation, "Cannot create payment for a draft or canceled sale return.");

        var overpaymentError = ValidateOverpayment(alreadyPaid, command.Amount, reference.TotalAmount, $"SALES_RETURN:{command.ReferenceId}");
        if (overpaymentError is not null)
            return overpaymentError;

        var sale = await saleRepository.GetByIdAsync(reference.SaleId, cancellationToken);
        if (sale is null)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.NotFound, "Original sale for this return was not found.");

        var totalPaidOnSale = await paymentRepository.GetTotalAmountByReferenceAsync(
            PaymentReferenceType.SALE, reference.SaleId, cancellationToken);

        var goodsKept = Math.Max(0, sale.TotalAmount - reference.TotalAmount);
        var overpaid = totalPaidOnSale - goodsKept;
        var maxRefund = Math.Max(0, overpaid);

        if (alreadyPaid + command.Amount > maxRefund)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Cannot refund {command.Amount}. Maximum refundable amount is {maxRefund} — the customer has only overpaid by that amount for the goods they kept.");

        return null;
    }

    private static ServiceResult<PaymentDto>? ValidatePurchaseReturn(PurchaseReturnReferenceInfo reference, CreatePaymentCommand command, decimal alreadyPaid)
    {
        return ValidateOverpayment(alreadyPaid, command.Amount, reference.TotalAmount, $"PURCHASE_RETURN:{command.ReferenceId}");
    }
}
