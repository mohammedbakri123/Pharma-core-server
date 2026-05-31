using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Interfaces;

public interface IPosStockService
{
    Task<ServiceResult<PosStockDto>> ExecuteAsync(PosStockQuery query, CancellationToken cancellationToken = default);
}
