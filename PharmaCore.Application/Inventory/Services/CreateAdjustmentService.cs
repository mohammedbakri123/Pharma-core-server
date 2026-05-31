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
    IBatchRepository batchRepository,
    IStockMovementRepository stockMovementRepository,
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

            // 1. Validate Batch Existence and Stock
            var batch = await batchRepository.GetByIdAsync(command.BatchId, cancellationToken);
            if (batch == null)
                return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.NotFound, "Batch not found.");

            var stockType = command.Type == 1 ? StockMovementType.IN : StockMovementType.OUT;

            // 2. Create Adjustment Record
            var adjustment = Adjustment.Create(
                command.MedicineId,
                command.BatchId,
                command.Quantity,
                stockType,
                command.UserId,
                command.Reason);

            var createdAdjustment = await adjustmentRepository.AddAsync(adjustment, cancellationToken);

            // 3. Update Batch Stock
            if (stockType == StockMovementType.IN)
            {
                batch.IncreaseStock(command.Quantity);
            }
            else
            {
                batch.DecreaseStock(command.Quantity);
            }
            await batchRepository.UpdateAsync(batch, cancellationToken);

            // 4. Create Stock Movement Record
            var stockMovement = StockMovement.Create(
                command.MedicineId,
                command.BatchId,
                command.Quantity,
                stockType,
                StockMovementReferenceType.ADJUSTMENT,
                createdAdjustment.AdjustmentId);

            var createdMovement = await stockMovementRepository.AddAsync(stockMovement, cancellationToken);

            logger.LogInformation("Created adjustment {AdjustmentId} and movement {MovementId} for batch {BatchId}", 
                createdAdjustment.AdjustmentId, createdMovement.StockMovementId, command.BatchId);

            return ServiceResult<AdjustmentWithStockMovementDto>.Ok(new AdjustmentWithStockMovementDto(
                createdAdjustment.AdjustmentId,
                createdAdjustment.MedicineId,
                createdAdjustment.BatchId,
                createdAdjustment.Quantity,
                (int)createdAdjustment.Type,
                createdAdjustment.Reason ?? "",
                createdAdjustment.UserId,
                createdAdjustment.CreatedAt,
                new StockMovementDto(
                    createdMovement.StockMovementId, 
                    createdMovement.MedicineId, 
                    createdMovement.BatchId, 
                    createdMovement.Quantity, 
                    (int)createdMovement.Type, 
                    (int)createdMovement.ReferenceType, 
                    createdMovement.ReferenceId, 
                    createdMovement.CreatedAt ?? DateTime.UtcNow)));
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
