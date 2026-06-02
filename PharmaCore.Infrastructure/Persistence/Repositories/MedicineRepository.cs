using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Infrastructure.Utilities;
using MedicineModel = PharmaCore.Infrastructure.Models.Medicine;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class MedicineRepository(ApplicationDbContext dbContext) : IMedicineRepository
{
    public async Task<Medicine?> GetByIdAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Medicines.AsNoTracking()
            .FirstOrDefaultAsync(e => e.MedicineId == medicineId && e.IsDeleted != true, cancellationToken);
            return model is null ? null : Map(model);
    }

    public async Task<Medicine?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        var trimmed = barcode.Trim();
        var model = await dbContext.Medicines.AsNoTracking()
            .FirstOrDefaultAsync(e => e.Barcode == trimmed && e.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    public async Task<PagedResult<Medicine>> ListAsync(
        int page,
        int limit,
        string? search,
        MedicineUnit? unit,
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Medicines
            .AsNoTracking()
            .Where(m => m.IsDeleted != true);

        query = ApplyFilters(query, search, unit, categoryId);

        var total = await query.CountAsync(cancellationToken);

        var models = await query
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<Medicine>(
            models.Select(Map).ToList(),
            total,
            page,
            limit);
    }

    public async Task<PagedResult<Medicine>> ListDeletedAsync(
        int page,
        int limit,
        string? search,
        MedicineUnit? unit,
        int? categoryId,
        CancellationToken cancellationToken = default)
    {
        var query = dbContext.Medicines
            .AsNoTracking()
            .Where(m => m.IsDeleted == true);

        query = ApplyFilters(query, search, unit, categoryId);

        var total = await query.CountAsync(cancellationToken);

        var models = await query
            .OrderByDescending(m => m.DeletedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<Medicine>(
            models.Select(Map).ToList(),
            total,
            page,
            limit);
    }

    public async Task<Medicine> AddAsync(Medicine entity, CancellationToken cancellationToken = default)
    {
        var model = new MedicineModel
        {
            Name = entity.Name,
            ArabicName = entity.ArabicName,
            Barcode = entity.Barcode,
            CategoryId = entity.CategoryId,
            Unit = entity.Unit.HasValue ? (short)entity.Unit.Value : (short)0,
        };
        dbContext.Medicines.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<Medicine> UpdateAsync(Medicine entity, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Medicines.FindAsync([entity.MedicineId], cancellationToken: cancellationToken);
        if (model is null)
            throw new KeyNotFoundException($"Medicine with ID {entity.MedicineId} not found.");

        model.Name = entity.Name;
        model.ArabicName = entity.ArabicName;
        model.Barcode = entity.Barcode;
        model.CategoryId = entity.CategoryId;
        model.Unit = entity.Unit.HasValue ? (short)entity.Unit.Value : (short)0;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Medicines.FindAsync([medicineId], cancellationToken: cancellationToken);
        if (model is null) return false;

        model.IsDeleted = true;
        model.DeletedAt = DateTimeHelper.GetCurrentTimestamp();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> RestoreDeletedAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Medicines.FindAsync([medicineId], cancellationToken: cancellationToken);
        if (model is null) return false;

        model.IsDeleted = false;
        model.DeletedAt = null;
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Medicines.FindAsync([medicineId], cancellationToken: cancellationToken);
        if (model is null) return false;

        dbContext.Remove(model);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsByNameAsync(string? name, int? excludeMedicineId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = dbContext.Medicines.AsNoTracking()
            .Where(m => m.Name.ToLower() == name.ToLower().Trim() && m.IsDeleted != true);

        if (excludeMedicineId.HasValue)
            query = query.Where(m => m.MedicineId != excludeMedicineId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByBarcodeAsync(string? barcode, int? excludeMedicineId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        var query = dbContext.Medicines.AsNoTracking()
            .Where(m => m.Barcode == barcode && m.IsDeleted != true);

        if (excludeMedicineId.HasValue)
            query = query.Where(m => m.MedicineId != excludeMedicineId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    private static IQueryable<MedicineModel> ApplyFilters(
        IQueryable<MedicineModel> query,
        string? search,
        MedicineUnit? unit,
        int? categoryId)
    {
        if (!string.IsNullOrWhiteSpace(search))
        {
            var trimmed = search.Trim();
            query = query.Where(m =>
                m.Name.Contains(trimmed) ||
                (m.ArabicName != null && m.ArabicName.Contains(trimmed)) ||
                (m.Barcode != null && m.Barcode.Contains(trimmed)));
        }

        if (unit.HasValue)
        {
            query = query.Where(m => m.Unit == (short)unit.Value);
        }

        if (categoryId.HasValue)
        {
            query = query.Where(m => m.CategoryId == categoryId.Value);
        }

        return query;
    }

    private static Medicine Map(MedicineModel model)
    {
        return Medicine.Rehydrate(
            model.MedicineId,
            model.Name,
            model.ArabicName,
            model.Barcode,
            model.CategoryId,
            (MedicineUnit?)model.Unit,
            model.CreatedAt ?? DateTimeHelper.GetCurrentTimestamp(),
            model.IsDeleted ?? false,
            model.DeletedAt);
    }

}
