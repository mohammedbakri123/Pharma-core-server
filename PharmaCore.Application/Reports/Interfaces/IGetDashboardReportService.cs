using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Interfaces;

public interface IGetDashboardReportService
{
    Task<ServiceResult<DashboardReportDto>> ExecuteAsync(
        DateTime? from,
        DateTime? to,
        CancellationToken cancellationToken = default);
}
