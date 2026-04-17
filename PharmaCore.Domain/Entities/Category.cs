namespace PharmaCore.Domain.Entities;

public sealed class Category
{
    private Category(
        int categoryId,
        string name,
        string? arabicName,
        bool isDeleted,
        DateTime? deletedAt)
    {
        CategoryId = categoryId;
        Name = ValidateName(name);
        ArabicName = NormalizeOptional(arabicName);
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int CategoryId { get; private set; }

    public string Name { get; private set; }

    public string? ArabicName { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    // 🔹 Factory
    public static Category Create(string name, string? arabicName)
    {
        return new Category(
            0,
            name,
            arabicName,
            false,
            null);
    }

    // 🔹 Rehydrate
    public static Category Rehydrate(
        int categoryId,
        string name,
        string? arabicName,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new Category(
            categoryId,
            name,
            arabicName,
            isDeleted,
            deletedAt);
    }

    // 🔹 Behavior (no generic Update)

    public void ChangeName(string name)
    {
        EnsureNotDeleted();
        Name = ValidateName(name);
    }

    public void ChangeArabicName(string? arabicName)
    {
        EnsureNotDeleted();
        ArabicName = NormalizeOptional(arabicName);
    }

    public void MarkDeleted()
    {
        if (IsDeleted)
            return;

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    // 🔹 Helpers

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Cannot modify a deleted category.");
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Category name is required.");

        return name.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}