using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;

public sealed class Sale
{
    private Sale(
        int saleId,
        int? userId,
        int? customerId,
        SaleStatus status,
        decimal totalAmount,
        decimal discount,
        DateTime createdAt,
        string? note,
        bool isDeleted,
        DateTime? deletedAt)
    {
        SaleId = saleId;
        UserId = userId;
        CustomerId = customerId;
        Status = status;
        TotalAmount = totalAmount;
        Discount = discount;
        CreatedAt = createdAt;
        Note = NormalizeOptional(note);
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int SaleId { get; private set; }

    public int? UserId { get; private set; }

    public int? CustomerId { get; private set; }

    public SaleStatus Status { get; private set; }

    public decimal TotalAmount { get; private set; }

    public decimal Discount { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public string? Note { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    // 🔹 Factory (new Sale)
    public static Sale Create(int? userId, int? customerId, string? note)
    {
        return new Sale(
            0,
            userId,
            customerId,
            SaleStatus.DRAFT,
            0m,
            0m,
            DateTime.UtcNow,
            note,
            false,
            null);
    }

    // 🔹 Rehydrate (from DB)
    public static Sale Rehydrate(
        int saleId,
        int? userId,
        int? customerId,
        SaleStatus status,
        decimal totalAmount,
        decimal discount,
        DateTime createdAt,
        string? note,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new Sale(
            saleId,
            userId,
            customerId,
            status,
            totalAmount,
            discount,
            createdAt,
            note,
            isDeleted,
            deletedAt);
    }

    // 🔹 Behavior

    public void AssignCustomer(int customerId)
    {
        EnsureNotDeleted();
        CustomerId = customerId;
    }

    public void AssignUser(int userId)
    {
        EnsureNotDeleted();
        UserId = userId;
    }

    public void AddAmount(decimal amount)
    {
        EnsureNotDeleted();

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.");

        TotalAmount += amount;
    }

    public void ApplyDiscount(decimal discount)
    {
        EnsureNotDeleted();

        if (discount < 0)
            throw new ArgumentException("Discount cannot be negative.");

        Discount = discount;
    }

    public void Complete()
    {
        EnsureNotDeleted();

        if (TotalAmount <= 0)
            throw new InvalidOperationException("Cannot complete sale with zero total.");

        Status = SaleStatus.COMPLETED;
    }

    public void Cancel()
    {
        EnsureNotDeleted();
        Status = SaleStatus.CANCELLED;
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

    // 🔹 Helpers

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted sale.");
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}