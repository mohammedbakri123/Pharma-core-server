using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.POS.Requests;

public sealed record PosCheckoutCommand(
    int? UserId,
    int? CustomerId,
    decimal Discount,
    string? Note,
    PaymentMethod PaymentMethod,
    decimal PaymentAmount,
    IReadOnlyList<PosCheckoutItem> Items);

public sealed record PosCheckoutItem(int MedicineId, int Quantity);
