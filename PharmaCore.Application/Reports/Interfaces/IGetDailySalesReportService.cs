using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Interfaces;

public interface IGetDailySalesReportService
{
    Task<ServiceResult<DailySalesReportDto>> ExecuteAsync(DateTime? date, CancellationToken cancellationToken = default);
}
