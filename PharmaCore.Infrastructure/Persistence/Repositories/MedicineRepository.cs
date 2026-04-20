using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using MedicineModel = PharmaCore.Infrastructure.Models.Medicine;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class MedicineRepository : IMedicineRepository
{
    private readonly ApplicationDbContext _dbContext;

    public MedicineRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Medicine?> GetByIdAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Medicines.AsNoTracking()
            .FirstOrDefaultAsync(e => e.MedicineId == medicineId && e.IsDeleted != true, cancellationToken);
return model is null ? null : Map(model);
    }

    public async Task<IEnumerable<Medicine>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Medicines
            .AsNoTracking()
            .Where(m => m.IsDeleted != true)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
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
        _dbContext.Medicines.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<Medicine> UpdateAsync(Medicine entity, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Medicines.FindAsync([entity.MedicineId], cancellationToken: cancellationToken);
        if (model is null)
            throw new KeyNotFoundException($"Medicine with ID {entity.MedicineId} not found.");

        model.Name = entity.Name;
        model.ArabicName = entity.ArabicName;
        model.Barcode = entity.Barcode;
        model.CategoryId = entity.CategoryId;
        model.Unit = entity.Unit.HasValue ? (short)entity.Unit.Value : (short)0;
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Medicines.FindAsync([medicineId], cancellationToken: cancellationToken);
        if (model is null) return false;

        model.IsDeleted = true;
        model.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int medicineId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Medicines.FindAsync([medicineId], cancellationToken: cancellationToken);
        if (model is null) return false;

        _dbContext.Remove(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    // public async Task<IEnumerable<Medicine>> GetPagedAsync(int page, int limit, string? searchTerm, MedicineUnit? unit, int? categoryId, CancellationToken cancellationToken = default)
    // {
    //     var query = BuildQuery(searchTerm, unit, categoryId);
    //     var models = await query
    //         .OrderByDescending(m => m.CreatedAt)
    //         .Skip((page - 1) * limit)
    //         .Take(limit)
    //         .ToListAsync(cancellationToken);
    //         
    //     return models.Select(Map);
    // }

    public async Task<bool> ExistsByNameAsync(string? name, int? excludeMedicineId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var query = _dbContext.Medicines.AsNoTracking()
            .Where(m => m.Name.ToLower() == name.ToLower().Trim() && m.IsDeleted != true);

        if (excludeMedicineId.HasValue)
            query = query.Where(m => m.MedicineId != excludeMedicineId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByBarcodeAsync(string? barcode, int? excludeMedicineId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        var query = _dbContext.Medicines.AsNoTracking()
            .Where(m => m.Barcode == barcode && m.IsDeleted != true);

        if (excludeMedicineId.HasValue)
            query = query.Where(m => m.MedicineId != excludeMedicineId.Value);

        return await query.AnyAsync(cancellationToken);
    }
    
    private IQueryable<MedicineModel> BuildQuery(string? searchTerm, MedicineUnit? unit, int? categoryId)
    {
        var query = _dbContext.Medicines.AsNoTracking().Where(e => e.IsDeleted != true);
        
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(m => m.Name.Contains(searchTerm) || 
                                   (m.ArabicName != null && m.ArabicName.Contains(searchTerm)) ||
                                   (m.Barcode != null && m.Barcode.Contains(searchTerm)));
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
            model.CreatedAt ?? DateTime.UtcNow, 
            model.IsDeleted ?? false, 
            model.DeletedAt);
    }
}
