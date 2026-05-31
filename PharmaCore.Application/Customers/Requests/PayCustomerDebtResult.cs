using PharmaCore.Application.Customers.Dtos;

namespace PharmaCore.Application.Customers.Requests;

public sealed record PayCustomerDebtResult(
    int PaymentId,
    decimal Amount,
    short Method,
    DateTime? CreatedAt,
    IReadOnlyList<AppliedSalePayment> AppliedToSales,
    CustomerBalanceSummary CustomerBalance);

public sealed record AppliedSalePayment(
    int SaleId,
    decimal AmountApplied,
    decimal RemainingBalance);

public sealed record CustomerBalanceSummary(
    decimal TotalDebt,
    decimal NewBalance);
