using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Interfaces;

public interface IGetProfitReportService
{
    Task<ServiceResult<ProfitReportDto>> ExecuteAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
