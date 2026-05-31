namespace PharmaCore.Application.Payments.Dtos;

public sealed record PaymentsByReferenceDto(
    IReadOnlyList<PaymentDto> Payments,
    decimal TotalPaid);
