using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.PurchaseReturns.Dtos;
using PharmaCore.Application.PurchaseReturns.Interfaces;
using PharmaCore.Application.PurchaseReturns.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.PurchaseReturns.Services;

public class ListPurchaseReturnsService(
    IPurchaseReturnRepository purchaseReturnRepository,
    IPaymentRepository paymentRepository,
    ILogger<ListPurchaseReturnsService> logger)
    : IListPurchaseReturnsService
{
    public async Task<ServiceResult<List<PurchaseReturnDto>>> ExecuteAsync(ListPurchaseReturnsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var returns = await purchaseReturnRepository.ListAsync(cancellationToken);
            var filtered = returns.Where(r => r.PurchaseId == query.PurchaseId).ToList();

            var returnIds = filtered.Select(r => r.PurchaseReturnId).ToList();
            var refundPayments = (await paymentRepository.GetByReferencesAsync(
                PaymentReferenceType.PURCHASE_RETURN, returnIds, cancellationToken)).ToList();

            var dtos = filtered.Select(r =>
            {
                var refundPayment = refundPayments.FirstOrDefault(p => p.ReferenceId == r.PurchaseReturnId);
                return new PurchaseReturnDto(
                    r.PurchaseReturnId,
                    r.PurchaseId,
                    r.SupplierId,
                    r.UserId,
                    r.TotalAmount,
                    r.Note,
                    r.CreatedAt,
                    r.Items.Select(i => new PurchaseReturnItemDto(
                        i.PurchaseReturnItemId,
                        i.PurchaseItemId,
                        i.BatchId,
                        i.Quantity,
                        i.UnitPrice,
                        i.TotalPrice)).ToList(),
                    refundPayment?.PaymentId
                );
            }).ToList();

            return ServiceResult<List<PurchaseReturnDto>>.Ok(dtos);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing purchase returns for purchase {PurchaseId}", query.PurchaseId);
            return ServiceResult<List<PurchaseReturnDto>>.Fail(ServiceErrorType.ServerError, $"Error listing returns: {e.Message}");
        }
    }
}
