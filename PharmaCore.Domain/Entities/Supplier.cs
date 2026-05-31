namespace PharmaCore.Domain.Entities;

public sealed class Supplier
{
    private Supplier(
        int supplierId,
        string name,
        string? phoneNumber,
        string? address,
        bool? isDeleted,
        DateTime? createdAt,
        DateTime? deletedAt)
    {
        SupplierId = supplierId;
        Name = ValidateName(name, nameof(name));
        PhoneNumber = NormalizeOptional(phoneNumber);
        Address = NormalizeOptional(address);
        IsDeleted = isDeleted;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }

    public int SupplierId { get; private set; }
    public string Name { get; private set; }
    public string? PhoneNumber { get; private set; }
    public string? Address { get; private set; }
    public bool? IsDeleted { get; private set; }
    public DateTime? CreatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Supplier Create(string name, string? phoneNumber, string? address)
    {
        return new Supplier(0, name, phoneNumber, address, false, null, null);
    }

    public static Supplier Rehydrate(
        int supplierId,
        string name,
        string? phoneNumber,
        string? address,
        bool? isDeleted,
        DateTime? createdAt,
        DateTime? deletedAt)
    {
        return new Supplier(supplierId, name, phoneNumber, address, isDeleted, createdAt, deletedAt);
    }

    public void Update(string? name, string? phoneNumber, string? address)
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
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    public void SetPersistedState(int supplierId, DateTime? createdAt)
    {
        SupplierId = supplierId;
        CreatedAt = createdAt;
    }

    private static string ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Supplier name is required.", paramName);
        }

        return name.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}