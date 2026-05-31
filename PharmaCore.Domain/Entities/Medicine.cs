using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;

public sealed class Medicine
{
    private Medicine(
        int medicineId,
        string name,
        string? arabicName,
        string? barcode,
        int? categoryId,
        MedicineUnit? unit,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        MedicineId = medicineId;
        Name = ValidateName(name);
        ArabicName = NormalizeOptional(arabicName);
        Barcode = NormalizeOptional(barcode);
        CategoryId = categoryId;
        Unit = unit;
        CreatedAt = createdAt;
        IsDeleted = isDeleted;
        DeletedAt = deletedAt;
    }

    public int MedicineId { get; private set; }

    public string Name { get; private set; }

    public string? ArabicName { get; private set; }

    public string? Barcode { get; private set; }

    public int? CategoryId { get; private set; }

    public MedicineUnit? Unit { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    // 🔹 Factory
    public static Medicine Create(
        string name,
        string? arabicName,
        string? barcode,
        int? categoryId,
        MedicineUnit? unit)
    {
        return new Medicine(
            0,
            name,
            arabicName,
            barcode,
            categoryId,
            unit,
            DateTime.UtcNow,
            false,
            null);
    }

    // 🔹 Rehydrate
    public static Medicine Rehydrate(
        int medicineId,
        string name,
        string? arabicName,
        string? barcode,
        int? categoryId,
        MedicineUnit? unit,
        DateTime createdAt,
        bool isDeleted,
        DateTime? deletedAt)
    {
        return new Medicine(
            medicineId,
            name,
            arabicName,
            barcode,
            categoryId,
            unit,
            createdAt,
            isDeleted,
            deletedAt);
    }

    // 🔹 Behavior (NO generic Update)

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

    public void SetBarcode(string? barcode)
    {
        EnsureNotDeleted();
        Barcode = NormalizeOptional(barcode);
    }

    public void AssignCategory(int? categoryId)
    {
        EnsureNotDeleted();
        CategoryId = categoryId;
    }

    public void SetUnit(MedicineUnit? unit)
    {
        EnsureNotDeleted();
        Unit = unit;
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
            throw new InvalidOperationException("Cannot modify a deleted medicine.");
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Medicine name is required.");

        return name.Trim();
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}