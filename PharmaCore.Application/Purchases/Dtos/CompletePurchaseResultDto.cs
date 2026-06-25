using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Purchases.Dtos;

public sealed record CompletePurchaseResultDto(
    int PurchaseId,
    PurchaseStatus Status,
    decimal TotalAmount,
    DateTime CompletedAt,
    int StockMovementsCreated,
    int PaymentId);
