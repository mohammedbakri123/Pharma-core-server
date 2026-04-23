using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Interfaces;

public interface IGetPaymentsReportService
{
    Task<ServiceResult<PaymentsReportDto>> ExecuteAsync(DateTime? from, DateTime? to, CancellationToken cancellationToken = default);
}
