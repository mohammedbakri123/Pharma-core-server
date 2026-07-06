using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.PurchaseReturns.Dtos;

public sealed record PurchaseReturnDetailsDto(
    int PurchaseReturnId,
    int? PurchaseId,
    int? SupplierId,
    int? UserId,
    PurchaseReturnStatus Status,
    decimal TotalAmount,
    string? Note,
    DateTime CreatedAt,
    IReadOnlyList<PurchaseReturnItemDto> Items);
