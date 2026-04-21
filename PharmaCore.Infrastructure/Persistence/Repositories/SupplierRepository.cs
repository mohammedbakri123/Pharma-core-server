using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Domain.Entities;
using PharmaCore.Infrastructure.Utilities;
using SupplierModel = PharmaCore.Infrastructure.Models.Supplier;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class SupplierRepository : ISupplierRepository
{
    private readonly ApplicationDbContext _dbContext;

    public SupplierRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Supplier?> GetByIdAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Suppliers.AsNoTracking()
            .FirstOrDefaultAsync(e => e.SupplierId == supplierId && e.IsDeleted != true, cancellationToken);
        return model is null ? null : Map(model);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Suppliers.AsNoTracking()
            .Where(e => e.Name.ToLower() == name.ToLower() && e.IsDeleted != true);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.SupplierId != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Supplier>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Suppliers.AsNoTracking()
            .Where(e => e.IsDeleted != true)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<IEnumerable<Supplier>> ListDeletedAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Suppliers.AsNoTracking()
            .Where(e => e.IsDeleted == true)
            .OrderByDescending(c => c.DeletedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<Supplier> AddAsync(Supplier entity, CancellationToken cancellationToken = default)
    {
        var model = new SupplierModel
        {
            Name = entity.Name,
            PhoneNumber = entity.PhoneNumber,
            Address = entity.Address,
            CreatedAt = DateTimeHelper.GetCurrentTimestamp(),
        };
        _dbContext.Suppliers.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<Supplier> UpdateAsync(Supplier entity, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Suppliers.FindAsync([entity.SupplierId], cancellationToken: cancellationToken);
        if (model is null)
            throw new KeyNotFoundException($"Supplier with ID {entity.SupplierId} not found.");

        if (entity.Name != model.Name) model.Name = entity.Name;
        if (entity.PhoneNumber != model.PhoneNumber) model.PhoneNumber = entity.PhoneNumber;
        if (entity.Address != model.Address) model.Address = entity.Address;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE suppliers SET is_deleted = true, deleted_at = now() WHERE supplier_id = {supplierId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> RestoreDeletedAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE suppliers SET is_deleted = false, deleted_at = NULL WHERE supplier_id = {supplierId} AND is_deleted IS TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> HardDeleteAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM suppliers WHERE supplier_id = {supplierId}",
            cancellationToken);

        return affectedRows > 0;
    }

    private static Supplier Map(SupplierModel model) =>
        Supplier.Rehydrate(
            model.SupplierId,
            model.Name,
            model.PhoneNumber,
            model.Address,
            model.IsDeleted,
            model.CreatedAt,
            model.DeletedAt);
}