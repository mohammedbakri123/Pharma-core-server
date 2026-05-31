using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Interfaces;

public interface IGetSalesRangeReportService
{
    Task<ServiceResult<SalesRangeReportDto>> ExecuteAsync(DateTime from, DateTime to, CancellationToken cancellationToken = default);
}
