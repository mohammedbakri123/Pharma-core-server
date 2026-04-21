using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface ICreateAdjustmentService
{
    Task<ServiceResult<AdjustmentWithStockMovementDto>> ExecuteAsync(
        CreateAdjustmentCommand command,
        CancellationToken cancellationToken = default);
}
