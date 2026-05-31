namespace PharmaCore.Domain.Entities;

public sealed class SalesReturn
{
    private SalesReturn(
        int salesReturnId,
        int? saleId,
        int? customerId,
        int? userId,
        decimal totalAmount,
        string? note,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt,
        List<SalesReturnItem>? items)
    {
        SalesReturnId = salesReturnId;
        SaleId = saleId;
        CustomerId = customerId;
        UserId = userId;
        TotalAmount = totalAmount;
        Note = note;
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
        Items = items ?? new List<SalesReturnItem>();
    }

    public int SalesReturnId { get; private set; }

    public int? SaleId { get; private set; }

    public int? CustomerId { get; private set; }

    public int? UserId { get; private set; }

    public decimal TotalAmount { get; private set; }

    public string? Note { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public IReadOnlyList<SalesReturnItem> Items { get; private set; } = new List<SalesReturnItem>();

    public static SalesReturn Create(int? saleId, int? customerId, int? userId, string? note)
    {
        return new SalesReturn(
            0,
            saleId,
            customerId,
            userId,
            0m,
            NormalizeOptional(note),
            DateTime.UtcNow,
            false,
            null,
            new List<SalesReturnItem>());
    }

    public static SalesReturn Rehydrate(
        int salesReturnId,
        int? saleId,
        int? customerId,
        int? userId,
        decimal totalAmount,
        string? note,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt,
        List<SalesReturnItem>? items = null)
    {
        return new SalesReturn(
            salesReturnId,
            saleId,
            customerId,
            userId,
            totalAmount,
            note,
            createdAt,
            isDeleted,
            deletedAt,
            items);
    }

    public void AddItem(SalesReturnItem item)
    {
        EnsureNotDeleted();
        var items = new List<SalesReturnItem>(Items) { item };
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
            throw new InvalidOperationException("Cannot modify a deleted sales return.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}