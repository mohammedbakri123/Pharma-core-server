using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;

public sealed class Adjustment
{
    private Adjustment(
        int adjustmentId,
        int medicineId,
        int batchId,
        int quantity,
        StockMovementType type,
        string? reason,
        int userId,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        AdjustmentId = adjustmentId;
        MedicineId = ValidatePositive(medicineId, nameof(medicineId));
        BatchId = ValidatePositive(batchId, nameof(batchId));
        Quantity = ValidatePositive(quantity, nameof(quantity));
        Type = type;
        Reason = NormalizeOptional(reason);
        UserId = ValidatePositive(userId, nameof(userId));
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int AdjustmentId { get; private set; }
    public int MedicineId { get; private set; }
    public int BatchId { get; private set; }
    public int Quantity { get; private set; }
    public StockMovementType Type { get; private set; }
    public string? Reason { get; private set; }
    public int UserId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Adjustment Create(
        int medicineId,
        int batchId,
        int quantity,
        StockMovementType type,
        int userId,
        string? reason)
    {
        return new Adjustment(
            0,
            medicineId,
            batchId,
            quantity,
            type,
            reason,
            userId,
            DateTime.UtcNow,
            false,
            null);
    }

    public static Adjustment Rehydrate(
        int adjustmentId,
        int medicineId,
        int batchId,
        int quantity,
        StockMovementType type,
        string? reason,
        int userId,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new Adjustment(
            adjustmentId,
            medicineId,
            batchId,
            quantity,
            type,
            reason,
            userId,
            createdAt,
            isDeleted,
            deletedAt);
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private static int ValidatePositive(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException($"{paramName} must be greater than zero.", paramName);
        return value;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}