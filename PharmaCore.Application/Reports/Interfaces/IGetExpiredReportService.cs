using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Interfaces;

public interface IGetExpiredReportService
{
    Task<ServiceResult<ExpiredReportDto>> ExecuteAsync(DateTime? includeExpiredBefore, CancellationToken cancellationToken = default);
}
