namespace PharmaCore.Domain.Entities;

public sealed class Expense
{
    private Expense(
        int expenseId,
        int? userId,
        decimal amount,
        string? description,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        ExpenseId = expenseId;
        UserId = userId;
        Amount = ValidateAmount(amount);
        Description = NormalizeOptional(description);
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int ExpenseId { get; private set; }
    public int? UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public bool? IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Expense Create(
        int? userId,
        decimal amount,
        string? description)
    {
        return new Expense(0, userId, amount, description, null, false, null);
    }

    public static Expense Rehydrate(
        int expenseId,
        int? userId,
        decimal amount,
        string? description,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        return new Expense(expenseId, userId, amount, description, createdAt, isDeleted, deletedAt);
    }

    private static decimal ValidateAmount(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        return amount;
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}