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

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Customers.AsNoTracking()
            .Where(e => e.Name.ToLower() == name.ToLower() && e.IsDeleted != true);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.CustomerId != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<IEnumerable<Customer>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await _dbContext.Customers.AsNoTracking()
            .Where(e => e.IsDeleted != true)
            .OrderByDescending(c => c.CreatedAt)
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
        var affectedRows = await _dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE customers SET is_deleted = true, deleted_at = now() WHERE customer_id = {customerId} AND is_deleted IS NOT TRUE",
            cancellationToken);

        return affectedRows > 0;
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

    public async Task<CustomerDebtDto?> GetDebtAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsDeleted != true, cancellationToken);

        if (customer == null)
            return null;

        var totalSales = await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        var saleIds = await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .Select(s => s.SaleId)
            .ToListAsync(cancellationToken);

        var totalPaid = await _dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && saleIds.Contains(p.ReferenceId) && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        var totalReturns = await _dbContext.SalesReturns.AsNoTracking()
            .Where(r => r.CustomerId == customerId && r.IsDeleted != true)
            .SumAsync(r => (decimal?)r.TotalAmount, cancellationToken) ?? 0m;

        return new CustomerDebtDto(customerId, customer.Name, totalSales, totalPaid, totalSales - totalPaid - totalReturns);
    }

    public async Task<IReadOnlyList<UnpaidSaleDto>> GetUnpaidSalesAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var sales = await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true && s.TotalAmount > 0)
            .OrderBy(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        var saleIds = sales.Select(s => s.SaleId).ToList();
        var paidBySaleId = await _dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && saleIds.Contains(p.ReferenceId) && p.IsDeleted != true)
            .GroupBy(p => p.ReferenceId)
            .Select(group => new { SaleId = group.Key, Total = group.Sum(p => p.Amount) })
            .ToDictionaryAsync(item => item.SaleId, item => item.Total, cancellationToken);

        return sales
            .Select(sale =>
            {
                var paid = paidBySaleId.TryGetValue(sale.SaleId, out var totalPaid) ? totalPaid : 0m;
                var totalAmount = sale.TotalAmount ?? 0m;
                return new UnpaidSaleDto(sale.SaleId, totalAmount, paid, totalAmount - paid, sale.CreatedAt);
            })
            .Where(sale => sale.RemainingAmount > 0)
            .ToList();
    }

    public async Task<CustomerStatementDto> GetStatementAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsDeleted != true, cancellationToken);

        if (customer == null)
            return new CustomerStatementDto(customerId, "Unknown", Array.Empty<StatementEntryDto>(), 0m, 0m);

        var entries = new List<StatementEntryDto>();

        var salesQuery = _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true);
        if (from.HasValue) salesQuery = salesQuery.Where(s => s.CreatedAt >= from);
        if (to.HasValue) salesQuery = salesQuery.Where(s => s.CreatedAt <= to);

        var sales = await salesQuery.OrderBy(s => s.CreatedAt).ToListAsync(cancellationToken);
        foreach (var sale in sales)
        {
            entries.Add(new StatementEntryDto(sale.CreatedAt, "SALE", sale.SaleId, $"Sale #{sale.SaleId}", sale.TotalAmount ?? 0m, 0m, 0m));
        }

        var saleIds = await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .Select(s => s.SaleId)
            .ToListAsync(cancellationToken);

        var paymentsQuery = _dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && saleIds.Contains(p.ReferenceId) && p.IsDeleted != true);
        if (from.HasValue) paymentsQuery = paymentsQuery.Where(p => p.CreatedAt >= from);
        if (to.HasValue) paymentsQuery = paymentsQuery.Where(p => p.CreatedAt <= to);

        var payments = await paymentsQuery.OrderBy(p => p.CreatedAt).ToListAsync(cancellationToken);
        foreach (var payment in payments)
        {
            entries.Add(new StatementEntryDto(payment.CreatedAt, "PAYMENT", payment.PaymentId, payment.Description ?? "Payment", 0m, payment.Amount, 0m));
        }

        var returnsQuery = _dbContext.SalesReturns.AsNoTracking()
            .Where(r => r.CustomerId == customerId && r.IsDeleted != true);
        if (from.HasValue) returnsQuery = returnsQuery.Where(r => r.CreatedAt >= from);
        if (to.HasValue) returnsQuery = returnsQuery.Where(r => r.CreatedAt <= to);

        var returns = await returnsQuery.OrderBy(r => r.CreatedAt).ToListAsync(cancellationToken);
        foreach (var salesReturn in returns)
        {
            entries.Add(new StatementEntryDto(salesReturn.CreatedAt, "RETURN", salesReturn.SalesReturnId, salesReturn.Note ?? "Sales Return", 0m, salesReturn.TotalAmount ?? 0m, 0m));
        }

        var runningBalance = 0m;
        var finalEntries = entries
            .OrderBy(entry => entry.Date)
            .Select(entry =>
            {
                runningBalance += entry.Debit - entry.Credit;
                return new StatementEntryDto(entry.Date, entry.Type, entry.ReferenceId, entry.Description, entry.Debit, entry.Credit, runningBalance);
            })
            .ToList();

        return new CustomerStatementDto(customerId, customer.Name, finalEntries, 0m, runningBalance);
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
