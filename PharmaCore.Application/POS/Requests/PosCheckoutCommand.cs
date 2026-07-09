using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.POS.Requests;

public sealed record PosCheckoutCommand(
    int? UserId,
    int? CustomerId,
    decimal Discount,
    string? Note,
    IReadOnlyList<PosPaymentItem> Payments,
    IReadOnlyList<PosCheckoutItem> Items);

public sealed record PosCheckoutItem(int MedicineId, int Quantity);

public sealed record PosPaymentItem(PaymentMethod Method, decimal Amount);
