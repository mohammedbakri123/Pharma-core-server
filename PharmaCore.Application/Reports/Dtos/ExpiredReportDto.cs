using System.Collections.Generic;

namespace PharmaCore.Application.Reports.Dtos;

public sealed record ExpiredReportDto(
    IReadOnlyList<ExpiredItemDto> ExpiredItems,
    decimal TotalExpiredValue);
