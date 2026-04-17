using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;

public sealed class StockMovement
{
    private StockMovement(
        int stockMovementId,
        int medicineId,
        int batchId,
        int quantity,
        StockMovementType type,
        StockMovementReferenceType referenceType,
        int referenceId,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        StockMovementId = stockMovementId;
        MedicineId = ValidatePositive(medicineId, nameof(medicineId));
        BatchId = ValidatePositive(batchId, nameof(batchId));
        Quantity = ValidatePositive(quantity, nameof(quantity));
        Type = type;
        ReferenceType = referenceType;
        ReferenceId = ValidatePositive(referenceId, nameof(referenceId));
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int StockMovementId { get; private set; }
    public int MedicineId { get; private set; }
    public int BatchId { get; private set; }
    public int Quantity { get; private set; }
    public StockMovementType Type { get; private set; }
    public StockMovementReferenceType ReferenceType { get; private set; }
    public int ReferenceId { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public bool? IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static StockMovement Create(
        int medicineId,
        int batchId,
        int quantity,
        StockMovementType type,
        StockMovementReferenceType referenceType,
        int referenceId)
    {
        return new StockMovement(0, medicineId, batchId, quantity, type, referenceType, referenceId, DateTime.UtcNow, false, null);
    }

    public static StockMovement Rehydrate(
        int stockMovementId,
        int medicineId,
        int batchId,
        int quantity,
        StockMovementType type,
        StockMovementReferenceType referenceType,
        int referenceId,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        return new StockMovement(stockMovementId, medicineId, batchId, quantity, type, referenceType, referenceId, createdAt, isDeleted, deletedAt);
    }

    private static int ValidatePositive(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException($"{paramName} must be greater than zero.", paramName);

        return value;
    }
}
