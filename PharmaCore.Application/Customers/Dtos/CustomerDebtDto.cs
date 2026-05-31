namespace PharmaCore.Application.Customers.Dtos;

public sealed record CustomerDebtDto(
    int CustomerId,
    string CustomerName,
    decimal TotalSales,
    decimal TotalPaid,
    decimal TotalDebt);
