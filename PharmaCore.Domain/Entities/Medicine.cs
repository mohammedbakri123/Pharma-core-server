using PharmaCore.Domain.Enums;

namespace PharmaCore.Domain.Entities;



public sealed class Medicine
{

    private Medicine(int medicineId, string name, string? arabicName, string? barcode, int? categoryId, MedicineUnit? unit, bool? isDeleted, DateTime? createdAt, DateTime? deletedAt )
    {
        MedicineId = medicineId;
        Name = ValidateName(name, nameof(name));
        ArabicName = NormalizeOptional(arabicName);
        Barcode =  NormalizeOptional(barcode);
        CategoryId = categoryId;
        Unit = unit;
        IsDeleted = isDeleted;
        CreatedAt = createdAt;
        DeletedAt = deletedAt;
    }
    
    public int MedicineId { get; private set; }
    public string Name { get; private set; }
    public string? ArabicName { get; private set; }

    public string? Barcode { get; private set; }

    public int? CategoryId { get;  private set; }

    public MedicineUnit? Unit { get; private set; }


    public DateTime? CreatedAt { get; private set; }

    public bool? IsDeleted { get; private set; }

    public DateTime? DeletedAt { get;private set; }

    public static Medicine Create(string name, string? arabicName, string? barcode, int? categoryId, MedicineUnit? unit)
    {
        return new Medicine(0, name, arabicName, barcode, categoryId, unit, null, null, null);
    }
    public static Medicine Rehydrate(int medicineId,string name, string? arabicName, string? barcode, int? categoryId, MedicineUnit? unit, bool? isDeleted, DateTime? createdAt, DateTime? deletedAt)
    {
        return new Medicine(medicineId, name, arabicName, barcode, categoryId, unit, isDeleted, createdAt, deletedAt);
    }

    public void Update(string? name, string? arabicName, string? barcode, int? categoryId, MedicineUnit? unit)
    {
        if (name is not null)
        {
            Name = ValidateName(name, nameof(name));
        }

        if (arabicName is not null)
        {
            ArabicName = NormalizeOptional(arabicName);
        }

        if (barcode is not null)
        {
            Barcode = NormalizeOptional(barcode);
        }

        if (categoryId is not null)
        {
            CategoryId = categoryId;
        }

        if (unit is not null)
        {
            Unit = unit;
        }
    }
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
    }
    public void SetPersistedState(int medicineId, DateTime? createdAt)
    {
        MedicineId = medicineId;
        CreatedAt = createdAt;
    }
    private static string ValidateName(string name, string paramName)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Medicine name is required.", paramName);
        }

        return name.Trim();
    }
    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

}