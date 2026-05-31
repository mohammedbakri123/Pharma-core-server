using PharmaCore.Application.Reports.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Reports.Interfaces;

public interface IGetStockReportService
{
    Task<ServiceResult<StockReportDto>> ExecuteAsync(int? categoryId, CancellationToken cancellationToken = default);
}
