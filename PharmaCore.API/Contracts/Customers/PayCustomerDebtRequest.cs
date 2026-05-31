using PharmaCore.Domain.Enums;

namespace PharmaCore.API.Contracts.Customers;

public sealed record PayCustomerDebtRequest(
    decimal Amount,
    PaymentMethod Method,
    string? Description);
