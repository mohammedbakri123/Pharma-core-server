namespace PharmaCore.Application.Reports.Dtos;

public sealed record StockByCategoryDto(
    int CategoryId,
    string CategoryName,
    int TotalStock,
    decimal StockValue);
