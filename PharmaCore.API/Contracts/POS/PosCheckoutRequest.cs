using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Contracts.POS;

public sealed record PosCheckoutRequest(
    IReadOnlyList<PosCheckoutItemRequest> Items,
    IReadOnlyList<PosPaymentRequest> Payments,
    int? CustomerId,
    decimal Discount,
    string? Note);

public sealed record PosCheckoutItemRequest(int MedicineId, int Quantity);

public sealed record PosPaymentRequest(PaymentMethod Method, decimal Amount);
