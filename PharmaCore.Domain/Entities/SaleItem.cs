namespace PharmaCore.Domain.Entities;

public sealed class SaleItem
{
    private SaleItem(
        int saleItemId,
        int saleId,
        int medicineId,
        int batchId,
        int quantity,
        decimal unitPrice,
        decimal totalPrice,
        decimal purchasePrice)
    {
        SaleItemId = saleItemId;
        SaleId = saleId;
        MedicineId = medicineId;
        BatchId = batchId;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
        PurchasePrice = purchasePrice;
    }

    public int SaleItemId { get; private set; }

    public int SaleId { get; private set; }

    public int MedicineId { get; private set; }

    public int BatchId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice { get; private set; }

    public decimal PurchasePrice { get; private set; }

    public static SaleItem Create(int saleId, int medicineId, int batchId, int quantity, decimal unitPrice, decimal purchasePrice)
    {
        var totalPrice = quantity * unitPrice;
        return new SaleItem(0, saleId, medicineId, batchId, quantity, unitPrice, totalPrice, purchasePrice);
    }

    public static SaleItem Rehydrate(
        int saleItemId,
        int saleId,
        int medicineId,
        int batchId,
        int quantity,
        decimal unitPrice,
        decimal totalPrice,
        decimal purchasePrice)
    {
        return new SaleItem(saleItemId, saleId, medicineId, batchId, quantity, unitPrice, totalPrice, purchasePrice);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        Quantity = newQuantity;
        TotalPrice = Quantity * UnitPrice;
    }
}