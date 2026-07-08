using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.PurchaseReturns.Dtos;

public sealed record PurchaseReturnDto(
    int PurchaseReturnId,
    int? PurchaseId,
    int? SupplierId,
    int? UserId,
    PurchaseReturnStatus Status,
    decimal TotalAmount,
    string? Note,
    DateTime CreatedAt,
   
    int? RefundPaymentId
);