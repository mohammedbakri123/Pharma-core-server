using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class CreateAdjustmentService(
    IAdjustmentRepository adjustmentRepository,
    ILogger<CreateAdjustmentService> logger)
    : ICreateAdjustmentService
{
    public async Task<ServiceResult<AdjustmentWithStockMovementDto>> ExecuteAsync(
        CreateAdjustmentCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.Quantity <= 0)
                return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, "Quantity must be > 0.");

            if (command.Type != 1 && command.Type != 2)
                return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, "Type must be 1 (INCREASE) or 2 (DECREASE).");

            if (string.IsNullOrWhiteSpace(command.Reason))
                return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, "Reason is required.");

            var stockType = command.Type == 1 ? StockMovementType.IN : StockMovementType.OUT;

            var adjustment = Adjustment.Create(
                command.MedicineId,
                command.BatchId,
                command.Quantity,
                stockType,
                command.UserId,
                command.Reason);

            var created = await adjustmentRepository.AddAsync(adjustment, cancellationToken);

            logger.LogInformation("Created adjustment {AdjustmentId} for batch {BatchId}", created.AdjustmentId, command.BatchId);

            return ServiceResult<AdjustmentWithStockMovementDto>.Ok(new AdjustmentWithStockMovementDto(
                created.AdjustmentId,
                created.MedicineId,
                created.BatchId,
                created.Quantity,
                (int)created.Type,
                created.Reason ?? "",
                created.UserId,
                created.CreatedAt,
                new StockMovementDto(0, created.MedicineId, created.BatchId, created.Quantity, (int)created.Type, 4, created.AdjustmentId, created.CreatedAt)));
        }
        catch (InvalidOperationException e)
        {
            logger.LogWarning(e, "Invalid operation creating adjustment");
            return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating adjustment");
            return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
