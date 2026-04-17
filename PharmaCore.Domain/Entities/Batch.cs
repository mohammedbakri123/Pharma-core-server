namespace PharmaCore.Domain.Entities;

public sealed class Batch
{
    private Batch(
        int batchId,
        int medicineId,
        string? batchNumber,
        int quantityRemaining,
        int quantityEntered,
        decimal purchasePrice,
        decimal sellPrice,
        DateOnly? expireDate,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        BatchId = batchId;
        MedicineId = ValidatePositive(medicineId, nameof(medicineId));
        BatchNumber = NormalizeOptional(batchNumber);
        QuantityRemaining = ValidateNonNegative(quantityRemaining, nameof(quantityRemaining));
        QuantityEntered = ValidateNonNegative(quantityEntered, nameof(quantityEntered));
        PurchasePrice = ValidateNonNegativePrice(purchasePrice, nameof(purchasePrice));
        SellPrice = ValidateNonNegativePrice(sellPrice, nameof(sellPrice));
        ExpireDate = expireDate;
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int BatchId { get; private set; }
    public int MedicineId { get; private set; }
    public string? BatchNumber { get; private set; }
    public int QuantityRemaining { get; private set; }
    public int QuantityEntered { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal SellPrice { get; private set; }
    public DateOnly? ExpireDate { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public bool? IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Batch Create(
        int medicineId,
        string? batchNumber,
        int quantityEntered,
        decimal purchasePrice,
        decimal sellPrice,
        DateOnly? expireDate)
    {
        return new Batch(
            0,
            medicineId,
            batchNumber,
            quantityEntered,
            quantityEntered,
            purchasePrice,
            sellPrice,
            expireDate,
            DateTime.UtcNow,
            false,
            null);
    }

    public static Batch Rehydrate(
        int batchId,
        int medicineId,
        string? batchNumber,
        int quantityRemaining,
        int quantityEntered,
        decimal purchasePrice,
        decimal sellPrice,
        DateOnly? expireDate,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        return new Batch(
            batchId,
            medicineId,
            batchNumber,
            quantityRemaining,
            quantityEntered,
            purchasePrice,
            sellPrice,
            expireDate,
            createdAt,
            isDeleted,
            deletedAt);
    }

    public void DecreaseStock(int quantity)
    {
        EnsureNotDeleted();

        quantity = ValidatePositive(quantity, nameof(quantity));
        if (QuantityRemaining < quantity)
            throw new InvalidOperationException("Insufficient batch stock.");

        QuantityRemaining -= quantity;
    }

    public void IncreaseStock(int quantity)
    {
        EnsureNotDeleted();
        quantity = ValidatePositive(quantity, nameof(quantity));
        QuantityRemaining += quantity;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted == true)
            throw new InvalidOperationException("Cannot modify a deleted batch.");
    }

    private static int ValidatePositive(int value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentException($"{paramName} must be greater than zero.", paramName);

        return value;
    }

    private static int ValidateNonNegative(int value, string paramName)
    {
        if (value < 0)
            throw new ArgumentException($"{paramName} cannot be negative.", paramName);

        return value;
    }

    private static decimal ValidateNonNegativePrice(decimal value, string paramName)
    {
        if (value < 0)
            throw new ArgumentException($"{paramName} cannot be negative.", paramName);

        return value;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
