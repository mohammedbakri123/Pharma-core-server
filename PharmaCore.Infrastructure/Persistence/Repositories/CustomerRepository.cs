using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Entities;
using PharmaCore.Infrastructure.Utilities;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class CustomerRepository(ApplicationDbContext dbContext) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Customers.AsNoTracking()
            .FirstOrDefaultAsync(e => e.CustomerId == customerId && e.IsDeleted != true, cancellationToken);
        return model is null ? null : Map(model);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Customers.AsNoTracking()
            .Where(e => e.Name.ToLower() == name.ToLower() && e.IsDeleted != true);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.CustomerId != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Customers.AsNoTracking()
            .Where(e => e.IsDeleted != true)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<IEnumerable<Customer>> ListDeletedAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Customers.AsNoTracking()
            .Where(e => e.IsDeleted == true)
            .OrderByDescending(c => c.DeletedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<Customer> AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var model = new Models.Customer
        {
            Name = entity.Name,
            PhoneNumber = entity.PhoneNumber,
            Address = entity.Address,
            Note = entity.Note,
            CreatedAt = DateTimeHelper.GetCurrentTimestamp(),
        };
        dbContext.Customers.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<Customer> UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Customers.FindAsync([entity.CustomerId], cancellationToken: cancellationToken);
        if (model is null)
            throw new KeyNotFoundException($"Customer with ID {entity.CustomerId} not found.");

        if (entity.Name != model.Name) model.Name = entity.Name;
        if (entity.PhoneNumber != model.PhoneNumber) model.PhoneNumber = entity.PhoneNumber;
        if (entity.Address != model.Address) model.Address = entity.Address;
        if (entity.Note != model.Note) model.Note = entity.Note;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var deletedAt = DateTimeHelper.GetCurrentTimestamp();
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE customers SET is_deleted = true, deleted_at = now() WHERE customer_id = {customerId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> RestoreDeletedAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE customers SET is_deleted = false, deleted_at = NULL WHERE customer_id = {customerId} AND is_deleted IS TRUE",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<bool> HardDeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM customers WHERE customer_id = {customerId}",
            cancellationToken);

        return affectedRows > 0;
    }

    private static Customer Map(Models.Customer model) =>
        Customer.Rehydrate(
            model.CustomerId,
            model.Name,
            model.PhoneNumber,
            model.Address,
            model.Note,
            model.IsDeleted,
            model.CreatedAt,
            model.DeletedAt);
}
