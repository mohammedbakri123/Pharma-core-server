using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Contracts.Sales;

public sealed record CreateSaleRequest(
    int? CustomerId,
    string? Note,
    decimal? Discount);

public sealed record AddSaleItemRequest(
    int MedicineId,
    int Quantity,
    decimal? UnitPrice);

public sealed record UpdateSaleItemRequest(int Quantity);

public sealed record SalePaymentRequest(
    decimal Amount,
    PaymentMethod Method,
    string? Description);

public sealed record CompleteSaleRequest(IReadOnlyList<SalePaymentRequest>? Payments);
