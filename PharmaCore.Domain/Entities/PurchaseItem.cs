namespace PharmaCore.Domain.Entities;

public sealed class PurchaseItem
{
    private PurchaseItem(
        int purchaseItemId,
        int purchaseId,
        int medicineId,
        int batchId,
        int quantity,
        decimal purchasePrice,
        decimal sellPrice,
        DateOnly? expireDate)
    {
        PurchaseItemId = purchaseItemId;
        PurchaseId = purchaseId;
        MedicineId = medicineId;
        BatchId = batchId;
        Quantity = quantity;
        PurchasePrice = purchasePrice;
        SellPrice = sellPrice;
        ExpireDate = expireDate;
    }

    public int PurchaseItemId { get; private set; }
    public int PurchaseId { get; private set; }
    public int MedicineId { get; private set; }
    public int BatchId { get; private set; }
    public int Quantity { get; private set; }
    public decimal PurchasePrice { get; private set; }
    public decimal SellPrice { get; private set; }
    public DateOnly? ExpireDate { get; private set; }

    public decimal TotalPrice => Quantity * PurchasePrice;

    public static PurchaseItem Create(
        int purchaseId,
        int medicineId,
        int batchId,
        int quantity,
        decimal purchasePrice,
        decimal sellPrice,
        DateOnly? expireDate)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (purchasePrice <= 0)
            throw new ArgumentException("Purchase price must be greater than zero.", nameof(purchasePrice));
        if (sellPrice <= 0)
            throw new ArgumentException("Sell price must be greater than zero.", nameof(sellPrice));

        return new PurchaseItem(0, purchaseId, medicineId, batchId, quantity, purchasePrice, sellPrice, expireDate);
    }

    public static PurchaseItem Rehydrate(
        int purchaseItemId,
        int purchaseId,
        int medicineId,
        int batchId,
        int quantity,
        decimal purchasePrice,
        decimal sellPrice,
        DateOnly? expireDate)
    {
        return new PurchaseItem(purchaseItemId, purchaseId, medicineId, batchId, quantity, purchasePrice, sellPrice, expireDate);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
    }

    public void UpdatePrices(decimal purchasePrice, decimal sellPrice)
    {
        if (purchasePrice <= 0)
            throw new ArgumentException("Purchase price must be greater than zero.", nameof(purchasePrice));
        if (sellPrice <= 0)
            throw new ArgumentException("Sell price must be greater than zero.", nameof(sellPrice));

        PurchasePrice = purchasePrice;
        SellPrice = sellPrice;
    }
}