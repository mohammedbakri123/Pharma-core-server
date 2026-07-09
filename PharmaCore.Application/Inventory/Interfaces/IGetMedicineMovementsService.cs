using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface IGetMedicineMovementsService
{
    Task<ServiceResult<PagedResult<StockMovementDto>>> ExecuteAsync(GetMedicineMovementsQuery query, CancellationToken cancellationToken = default);
}