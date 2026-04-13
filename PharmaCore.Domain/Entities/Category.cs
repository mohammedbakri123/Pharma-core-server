namespace PharmaCore.Domain.Entities;

public sealed class Category
{
    private Category(
        int categoryId,
        string categoryName,
        string categoryArabicName,
        bool isDeleted,
        DateTime? deletedAt)
    {
        CategoryId = categoryId;
        CategoryName = ValidateName(categoryName, nameof(categoryName));
        CategoryArabicName = NormalizeOptional(categoryArabicName);
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int CategoryId { get; private set; }
    public string CategoryName { get; private set; }
    public string? CategoryArabicName { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public static Category Create(string categoryName, string? categoryArabicName)
    {
        return new Category(0, categoryName, categoryArabicName, false, null);
    }

    public static Category Rehydrate(
        int categoryId,
        string categoryName,
        string categoryArabicName,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new Category(categoryId, categoryName, categoryArabicName, isDeleted, deletedAt);
    }

    public void Update(string? categoryName, string? categoryArabicName)
    {
        if (categoryName is not null)
        {
            CategoryName = ValidateName(categoryName, nameof(categoryName));
        }

        if (categoryArabicName is not null)
        {
            CategoryArabicName = NormalizeOptional(categoryArabicName);
        }
    }

    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }

    private static string ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Category name is required.", paramName);
        }

        return name.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
