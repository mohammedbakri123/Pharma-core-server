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

            var statusError = ValidateReferenceStatus(reference, command.ReferenceType);
            if (statusError is not null)
                return statusError;

            var alreadyPaid = await paymentRepository.GetTotalAmountByReferenceAsync(
                command.ReferenceType, command.ReferenceId, cancellationToken);

            if (alreadyPaid + command.Amount > reference.TotalAmount)
                return ServiceResult<PaymentDto>.Fail(
                    ServiceErrorType.Validation,
                    $"Payment amount {command.Amount} exceeds remaining amount of {reference.TotalAmount - alreadyPaid} for {command.ReferenceType}:{command.ReferenceId}.");

            var paymentType = Payment.DeriveType(command.ReferenceType);
            var payment = Payment.Create(
                paymentType,
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

    private sealed record ReferenceInfo(decimal TotalAmount, object? Status);

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
        return sale is null ? null : new ReferenceInfo(sale.TotalAmount, sale.Status);
    }

    private async Task<ReferenceInfo?> FetchPurchaseAsync(int referenceId, CancellationToken cancellationToken)
    {
        var purchase = await purchaseRepository.GetByIdAsync(referenceId, cancellationToken);
        return purchase is null ? null : new ReferenceInfo(purchase.TotalAmount, purchase.Status);
    }

    private async Task<ReferenceInfo?> FetchExpenseAsync(int referenceId, CancellationToken cancellationToken)
    {
        var expense = await expenseRepository.GetByIdAsync(referenceId, cancellationToken);
        return expense is null ? null : new ReferenceInfo(expense.Amount, null);
    }

    private async Task<ReferenceInfo?> FetchSalesReturnAsync(int referenceId, CancellationToken cancellationToken)
    {
        var salesReturn = await salesReturnRepository.GetByIdAsync(referenceId, cancellationToken);
        return salesReturn is null ? null : new ReferenceInfo(salesReturn.TotalAmount, null);
    }

    private async Task<ReferenceInfo?> FetchPurchaseReturnAsync(int referenceId, CancellationToken cancellationToken)
    {
        var purchaseReturn = await purchaseReturnRepository.GetByIdAsync(referenceId, cancellationToken);
        return purchaseReturn is null ? null : new ReferenceInfo(purchaseReturn.TotalAmount, null);
    }

    private static ServiceResult<PaymentDto>? ValidateReferenceStatus(ReferenceInfo reference, PaymentReferenceType referenceType)
    {
        return referenceType switch
        {
            PaymentReferenceType.SALE => ValidateSaleStatus((SaleStatus)reference.Status!),
            PaymentReferenceType.PURCHASE => ValidatePurchaseStatus((PurchaseStatus)reference.Status!),
            _ => null
        };
    }

    private static ServiceResult<PaymentDto>? ValidateSaleStatus(SaleStatus status)
    {
       
        if (status != SaleStatus.COMPLETED)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Cannot create payment for a draft or canceled sale.");

        return null;
    }

    private static ServiceResult<PaymentDto>? ValidatePurchaseStatus(PurchaseStatus status)
    {
        if (status == PurchaseStatus.Cancelled)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Cannot create payment for a cancelled purchase.");

        if (status != PurchaseStatus.Completed)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Cannot create payment for a draft purchase.");

        return null;
    }
}
