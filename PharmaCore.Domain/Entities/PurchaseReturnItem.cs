namespace PharmaCore.Domain.Entities;

public sealed class PurchaseReturnItem
{
    private PurchaseReturnItem(
        int purchaseReturnItemId,
        int purchaseReturnId,
        int purchaseItemId,
        int batchId,
        int quantity,
        decimal unitPrice,
        decimal totalPrice)
    {
        PurchaseReturnItemId = purchaseReturnItemId;
        PurchaseReturnId = purchaseReturnId;
        PurchaseItemId = purchaseItemId;
        BatchId = batchId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
    }

    public int PurchaseReturnItemId { get; private set; }
    public int PurchaseReturnId { get; private set; }
    public int PurchaseItemId { get; private set; }
    public int BatchId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }

    public static PurchaseReturnItem Create(int purchaseReturnId, int purchaseItemId, int batchId, int quantity, decimal unitPrice)
    {
        var totalPrice = quantity * unitPrice;
        return new PurchaseReturnItem(0, purchaseReturnId, purchaseItemId, batchId, quantity, unitPrice, totalPrice);
    }

    public static PurchaseReturnItem Rehydrate(
        int purchaseReturnItemId,
        int purchaseReturnId,
        int purchaseItemId,
        int batchId,
        int quantity,
        decimal unitPrice,
        decimal totalPrice)
    {
        return new PurchaseReturnItem(purchaseReturnItemId, purchaseReturnId, purchaseItemId, batchId, quantity, unitPrice, totalPrice);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        Quantity = newQuantity;
        TotalPrice = Quantity * UnitPrice;
    }
}