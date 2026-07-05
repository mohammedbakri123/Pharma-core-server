using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Handlers;

internal sealed class SalePaymentCreateHandler(ISaleRepository saleRepository) : IPaymentCreateHandler
{
    public PaymentReferenceType ReferenceType => PaymentReferenceType.SALE;

    public async Task<ServiceResult<PaymentDto>?> ValidateAsync(
        CreatePaymentCommand command, decimal alreadyPaid, CancellationToken cancellationToken)
    {
        var sale = await saleRepository.GetByIdAsync(command.ReferenceId, cancellationToken);
        if (sale is null)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.NotFound, $"Sale with ID {command.ReferenceId} was not found.");

        if (sale.Status != SaleStatus.COMPLETED)
            return ServiceResult<PaymentDto>.Fail(ServiceErrorType.Validation, "Cannot create payment for a draft or canceled sale.");

        if (alreadyPaid + command.Amount > sale.TotalAmount)
            return ServiceResult<PaymentDto>.Fail(
                ServiceErrorType.Validation,
                $"Payment amount {command.Amount} exceeds remaining amount of {sale.TotalAmount - alreadyPaid} for SALE:{command.ReferenceId}.");

        return null;
    }
}
