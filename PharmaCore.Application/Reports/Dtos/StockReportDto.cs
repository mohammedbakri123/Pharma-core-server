using System.Collections.Generic;

namespace PharmaCore.Application.Reports.Dtos;

public sealed record StockReportDto(
    int TotalMedicines,
    int TotalBatches,
    decimal TotalStockValue,
    IReadOnlyList<StockByCategoryDto> StockByCategory,
    int LowStockCount,
    int ExpiredCount,
    int ExpiringSoonCount);
