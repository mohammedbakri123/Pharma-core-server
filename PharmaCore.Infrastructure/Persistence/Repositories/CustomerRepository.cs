using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class CustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Customer?> GetByIdAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Customers.AsNoTracking()
            .FirstOrDefaultAsync(e => e.CustomerId == customerId && e.IsDeleted != true, cancellationToken);
        return model is null ? null : Map(model);
    }

    public async Task<PagedResult<Customer>> ListAsync(int page, int limit, string? searchTerm, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers.AsNoTracking().Where(e => e.IsDeleted != true);

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(c => c.Name.Contains(searchTerm) || (c.PhoneNumber != null && c.PhoneNumber.Contains(searchTerm)));
        }

        var total = await query.CountAsync(cancellationToken);
        var models = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return new PagedResult<Customer>(models.Select(Map).ToList(), total, page, limit);
    }

    public async Task<Customer> AddAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var model = new Models.Customer
        {
            Name = entity.Name,
            PhoneNumber = entity.PhoneNumber,
            Address = entity.Address,
            Note = entity.Note,
            CreatedAt = DateTime.Now,
        };
        _dbContext.Customers.Add(model);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<Customer> UpdateAsync(Customer entity, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Customers.FindAsync([entity.CustomerId], cancellationToken: cancellationToken);
        if (model is null)
            throw new KeyNotFoundException($"Customer with ID {entity.CustomerId} not found.");

        if (entity.Name != model.Name) model.Name = entity.Name;
        if (entity.PhoneNumber != model.PhoneNumber) model.PhoneNumber = entity.PhoneNumber;
        if (entity.Address != model.Address) model.Address = entity.Address;
        if (entity.Note != model.Note) model.Note = entity.Note;

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Map(model);
    }

    public async Task<bool> SoftDeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var model = await _dbContext.Customers.FindAsync([customerId], cancellationToken: cancellationToken);
        if (model is null) return false;

        model.IsDeleted = true;
        model.DeletedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> HardDeleteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"DELETE FROM customers WHERE customer_id = {customerId}",
            cancellationToken);

        return affectedRows > 0;
    }

    public async Task<PagedResult<CustomerSaleDto>> GetSalesAsync(int customerId, int page, int limit, short? status, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true);

        if (status.HasValue)
            query = query.Where(s => s.Status == status.Value);

        var total = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderByDescending(s => s.CreatedAt)
            .Skip((page - 1) * limit)
            .Take(limit)
            .Select(s => new CustomerSaleDto(s.SaleId, s.Status, s.TotalAmount, s.Discount, s.CreatedAt, s.Note))
            .ToListAsync(cancellationToken);

        return new PagedResult<CustomerSaleDto>(items, total, page, limit);
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
