namespace PharmaCore.Domain.Entities;

public sealed class PurchaseReturn
{
    private PurchaseReturn(
        int purchaseReturnId,
        int? purchaseId,
        int? supplierId,
        int? userId,
        decimal totalAmount,
        string? note,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt,
        List<PurchaseReturnItem>? items)
    {
        PurchaseReturnId = purchaseReturnId;
        PurchaseId = purchaseId;
        SupplierId = supplierId;
        UserId = userId;
        TotalAmount = totalAmount;
        Note = NormalizeOptional(note);
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
        Items = items ?? new List<PurchaseReturnItem>();
    }

    public int PurchaseReturnId { get; private set; }
    public int? PurchaseId { get; private set; }
    public int? SupplierId { get; private set; }
    public int? UserId { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Note { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyList<PurchaseReturnItem> Items { get; private set; } = new List<PurchaseReturnItem>();

    public static PurchaseReturn Create(int? purchaseId, int? supplierId, int? userId, string? note)
    {
        return new PurchaseReturn(
            0,
            purchaseId,
            supplierId,
            userId,
            0m,
            NormalizeOptional(note),
            DateTime.UtcNow,
            false,
            null,
            new List<PurchaseReturnItem>());
    }

    public static PurchaseReturn Rehydrate(
        int purchaseReturnId,
        int? purchaseId,
        int? supplierId,
        int? userId,
        decimal totalAmount,
        string? note,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt,
        List<PurchaseReturnItem>? items = null)
    {
        return new PurchaseReturn(
            purchaseReturnId,
            purchaseId,
            supplierId,
            userId,
            totalAmount,
            note,
            createdAt,
            isDeleted,
            deletedAt,
            items);
    }

    public void AddItem(PurchaseReturnItem item)
    {
        EnsureNotDeleted();
        var items = new List<PurchaseReturnItem>(Items) { item };
        Items = items;
        TotalAmount += item.TotalPrice;
    }

    public void UpdateAmount(decimal amount)
    {
        EnsureNotDeleted();
        TotalAmount = amount;
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

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted purchase return.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}