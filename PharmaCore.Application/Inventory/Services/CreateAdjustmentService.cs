using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Inventory.Dtos;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Inventory.Services;

public class CreateAdjustmentService : ICreateAdjustmentService
{
    private readonly IAdjustmentRepository _adjustmentRepository;
    private readonly ILogger<CreateAdjustmentService> _logger;

    public CreateAdjustmentService(
        IAdjustmentRepository adjustmentRepository,
        ILogger<CreateAdjustmentService> logger)
    {
        _adjustmentRepository = adjustmentRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<AdjustmentWithStockMovementDto>> ExecuteAsync(
        int medicineId,
        int batchId,
        int quantity,
        int type,
        int userId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (quantity <= 0)
                return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, "Quantity must be > 0.");

            if (type != 1 && type != 2)
                return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, "Type must be 1 (INCREASE) or 2 (DECREASE).");

            if (string.IsNullOrWhiteSpace(reason))
                return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, "Reason is required.");

            var stockType = type == 1 ? StockMovementType.IN : StockMovementType.OUT;

            var adjustment = Adjustment.Create(
                medicineId,
                batchId,
                quantity,
                stockType,
                userId,
                reason);

            var created = await _adjustmentRepository.AddAsync(adjustment, cancellationToken);

            _logger.LogInformation("Created adjustment {AdjustmentId} for batch {BatchId}", created.AdjustmentId, batchId);

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
            _logger.LogWarning(e, "Invalid operation creating adjustment");
            return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating adjustment");
            return ServiceResult<AdjustmentWithStockMovementDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}