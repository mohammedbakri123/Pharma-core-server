using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;

public sealed class Payment
{
    private Payment(
        int paymentId,
        PaymentType type,
        PaymentReferenceType referenceType,
        int referenceId,
        PaymentMethod? method,
        int? userId,
        decimal amount,
        string? description,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        PaymentId = paymentId;
        Type = type;
        ReferenceType = referenceType;
        ReferenceId = ValidateReferenceId(referenceId);
        Method = method;
        UserId = userId;
        Amount = ValidateAmount(amount);
        Description = NormalizeOptional(description);
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int PaymentId { get; private set; }
    public PaymentType Type { get; private set; }
    public PaymentReferenceType ReferenceType { get; private set; }
    public int ReferenceId { get; private set; }
    public PaymentMethod? Method { get; private set; }
    public int? UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string? Description { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public bool? IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Payment Create(
        PaymentType type,
        PaymentReferenceType referenceType,
        int referenceId,
        PaymentMethod? method,
        int? userId,
        decimal amount,
        string? description)
    {
        return new Payment(0, type, referenceType, referenceId, method, userId, amount, description, null, false, null);
    }

    public static Payment Rehydrate(
        int paymentId,
        PaymentType type,
        PaymentReferenceType referenceType,
        int referenceId,
        PaymentMethod? method,
        int? userId,
        decimal amount,
        string? description,
        DateTime? createdAt,
        bool? isDeleted,
        DateTime? deletedAt)
    {
        return new Payment(paymentId, type, referenceType, referenceId, method, userId, amount, description, createdAt, isDeleted, deletedAt);
    }

    private static int ValidateReferenceId(int referenceId)
    {
        if (referenceId <= 0)
            throw new ArgumentException("Reference ID must be greater than zero.", nameof(referenceId));

        return referenceId;
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
