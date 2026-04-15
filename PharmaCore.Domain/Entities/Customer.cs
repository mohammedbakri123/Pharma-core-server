namespace PharmaCore.Domain.Entities;

public sealed class Customer
{
    private Customer(
        int customerId,
        string name,
        string? phoneNumber,
        string? address,
        string? note,
        bool? isDeleted,
        DateTime? createdAt,
        DateTime? deletedAt)
    {
        CustomerId = customerId;
        Name = ValidateName(name, nameof(name));
        PhoneNumber = NormalizeOptional(phoneNumber);
        Address = NormalizeOptional(address);
        Note = NormalizeOptional(note);
        IsDeleted = isDeleted;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }

    public int CustomerId { get; private set; }
    public string Name { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public string? Note { get; private set; }
    public bool? IsDeleted { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Customer Create(string name, string? phoneNumber, string? address, string? note)
    {
        return new Customer(0, name, phoneNumber, address, note, false, null, null);
    }

    public static Customer Rehydrate(
        int customerId,
        string name,
        string? phoneNumber,
        string? address,
        string? note,
        bool? isDeleted,
        DateTime? createdAt,
        DateTime? deletedAt)
    {
        return new Customer(customerId, name, phoneNumber, address, note, isDeleted, createdAt, deletedAt);
    }

    public void Update(string? name, string? phoneNumber, string? address, string? note)
    {
        if (name is not null)
        {
            Name = ValidateName(name, nameof(name));
        }

        if (phoneNumber is not null)
        {
            PhoneNumber = NormalizeOptional(phoneNumber);
        }

        if (address is not null)
        {
            Address = NormalizeOptional(address);
        }

        if (note is not null)
        {
            Note = NormalizeOptional(note);
        }
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void SetPersistedState(int customerId, DateTime? createdAt)
    {
        CustomerId = customerId;
        CreatedAt = createdAt;
    }

    private static string ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Customer name is required.", paramName);
        }

        return name.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
