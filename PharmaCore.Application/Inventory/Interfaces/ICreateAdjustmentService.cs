using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Interfaces;

public interface ICreateAdjustmentService
{
    Task<ServiceResult<AdjustmentWithStockMovementDto>> ExecuteAsync(
        int medicineId,
        int batchId,
        int quantity,
        int type,
        int userId,
        string reason,
        CancellationToken cancellationToken = default);
}