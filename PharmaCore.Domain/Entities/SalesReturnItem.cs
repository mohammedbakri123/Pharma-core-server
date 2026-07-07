namespace PharmaCore.Domain.Entities;

public sealed class SalesReturnItem
{
    private SalesReturnItem(
        int salesReturnItemId,
        int salesReturnId,
        int saleItemId,
        int batchId,
        string? batchNumber,
        string? medicineName,
        int quantity,
        decimal unitPrice,
        decimal totalPrice)
    {
        SalesReturnItemId = salesReturnItemId;
        SalesReturnId = salesReturnId;
        SaleItemId = saleItemId;
        BatchId = batchId;
        BatchNumber = batchNumber;
        MedicineName = medicineName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        TotalPrice = totalPrice;
    }

    public int SalesReturnItemId { get; private set; }

    public int SalesReturnId { get; private set; }

    public int SaleItemId { get; private set; }

    public int BatchId { get; private set; }

    public string? BatchNumber { get; private set; }

    public string? MedicineName { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    public decimal TotalPrice { get; private set; }

    public static SalesReturnItem Create(int salesReturnId, int saleItemId, int batchId, string? batchNumber, string? medicineName, int quantity, decimal unitPrice)
    {
        var totalPrice = quantity * unitPrice;
        return new SalesReturnItem(0, salesReturnId, saleItemId, batchId, batchNumber, medicineName, quantity, unitPrice, totalPrice);
    }

    public static SalesReturnItem Rehydrate(
        int salesReturnItemId,
        int salesReturnId,
        int saleItemId,
        int batchId,
        string? batchNumber,
        string? medicineName,
        int quantity,
        decimal unitPrice,
        decimal totalPrice)
    {
        return new SalesReturnItem(salesReturnItemId, salesReturnId, saleItemId, batchId, batchNumber, medicineName, quantity, unitPrice, totalPrice);
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.");

        Quantity = newQuantity;
        TotalPrice = Quantity * UnitPrice;
    }
}