using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.PurchaseReturns.Dtos;

public sealed record CompletePurchaseReturnResultDto(
    int PurchaseReturnId,
    PurchaseReturnStatus Status,
    decimal TotalAmount,
    DateTime CompletedAt,
    int StockMovementsCreated,
    int? RefundPaymentId);
