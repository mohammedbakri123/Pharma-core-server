using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;

public sealed class Purchase
{
    private Purchase(
        int purchaseId,
        int? supplierId,
        string? invoiceNumber,
        decimal totalAmount,
        PurchaseStatus status,
        DateTime createdAt,
        string? note,
        bool isDeleted,
        DateTime? deletedAt,
        List<PurchaseItem>? items)
    {
        PurchaseId = purchaseId;
        SupplierId = supplierId;
        InvoiceNumber = NormalizeOptional(invoiceNumber);
        TotalAmount = totalAmount;
        Status = status;
        CreatedAt = createdAt;
        Note = NormalizeOptional(note);
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
        Items = items ?? new List<PurchaseItem>();
    }

    public int PurchaseId { get; private set; }
    public int? SupplierId { get; private set; }
    public string? InvoiceNumber { get; private set; }
    public decimal TotalAmount { get; private set; }
    public PurchaseStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? Note { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyList<PurchaseItem> Items { get; private set; } = new List<PurchaseItem>();

    public static Purchase Create(int? supplierId, string? invoiceNumber, string? note)
    {
        return new Purchase(
            0,
            supplierId,
            invoiceNumber,
            0m,
            PurchaseStatus.DRAFT,
            DateTime.UtcNow,
            note,
            false,
            null,
            new List<PurchaseItem>());
    }

    public static Purchase Rehydrate(
        int purchaseId,
        int? supplierId,
        string? invoiceNumber,
        decimal totalAmount,
        PurchaseStatus status,
        DateTime createdAt,
        string? note,
        bool isDeleted,
        DateTime? deletedAt,
        List<PurchaseItem>? items = null)
    {
        return new Purchase(
            purchaseId,
            supplierId,
            invoiceNumber,
            totalAmount,
            status,
            createdAt,
            note,
            isDeleted,
            deletedAt,
            items);
    }

    public void AssignSupplier(int supplierId)
    {
        EnsureNotDeleted();
        SupplierId = supplierId;
    }

    public void SetInvoiceNumber(string? invoiceNumber)
    {
        EnsureNotDeleted();
        InvoiceNumber = NormalizeOptional(invoiceNumber);
    }

    public void AddAmount(decimal amount)
    {
        EnsureNotDeleted();

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        TotalAmount += amount;
    }

    public void Complete()
    {
        EnsureNotDeleted();

        if (TotalAmount <= 0)
            throw new InvalidOperationException("Cannot complete purchase with zero total.");

        Status = PurchaseStatus.COMPLETED;
    }

    public void Cancel()
    {
        EnsureNotDeleted();
        Status = PurchaseStatus.CANCELLED;
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void UpdateNote(string? note)
    {
        EnsureNotDeleted();
        Note = NormalizeOptional(note);
    }

    public void AddItem(PurchaseItem item)
    {
        EnsureNotDeleted();
        var items = new List<PurchaseItem>(Items) { item };
        Items = items;
        TotalAmount += item.TotalPrice;
    }

    public void RemoveItem(int purchaseItemId)
    {
        EnsureNotDeleted();
        var item = Items.FirstOrDefault(i => i.PurchaseItemId == purchaseItemId);
        if (item is null)
            throw new KeyNotFoundException($"Purchase item with ID {purchaseItemId} not found.");

        TotalAmount -= item.TotalPrice;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted purchase.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}