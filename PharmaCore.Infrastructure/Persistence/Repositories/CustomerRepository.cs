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

        if (customer == null) return null;

        var totalSales = await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .SumAsync(s => (decimal?)s.TotalAmount, cancellationToken) ?? 0m;

        // Payments linked to sales for this customer
        var saleIds = await _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true)
            .Select(s => s.SaleId)
            .ToListAsync(cancellationToken);

        var totalPaid = await _dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && saleIds.Contains(p.ReferenceId))
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

        // Subtract returns
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

        var result = new List<UnpaidSaleDto>();
        foreach (var sale in sales)
        {
            var paid = await _dbContext.Payments.AsNoTracking()
                .Where(p => p.ReferenceType == 1 && p.ReferenceId == sale.SaleId)
                .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

            var remaining = (sale.TotalAmount ?? 0m) - paid;
            if (remaining > 0)
            {
                result.Add(new UnpaidSaleDto(sale.SaleId, sale.TotalAmount ?? 0m, paid, remaining, sale.CreatedAt));
            }
        }

        return result;
    }

    public async Task<CustomerStatementDto> GetStatementAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var customer = await _dbContext.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.CustomerId == customerId && c.IsDeleted != true, cancellationToken);

        if (customer == null)
            return new CustomerStatementDto(customerId, "Unknown", Array.Empty<StatementEntryDto>(), 0, 0);

        var entries = new List<StatementEntryDto>();

        // Sales (debits)
        var salesQuery = _dbContext.Sales.AsNoTracking()
            .Where(s => s.CustomerId == customerId && s.IsDeleted != true);
        if (from.HasValue) salesQuery = salesQuery.Where(s => s.CreatedAt >= from);
        if (to.HasValue) salesQuery = salesQuery.Where(s => s.CreatedAt <= to);

        var sales = await salesQuery.OrderBy(s => s.CreatedAt).ToListAsync(cancellationToken);
        foreach (var s in sales)
        {
            entries.Add(new StatementEntryDto(s.CreatedAt, "SALE", s.SaleId, $"Sale #{s.SaleId}", s.TotalAmount ?? 0m, 0, 0));
        }

        // Payments (credits)
        var saleIds = sales.Select(s => s.SaleId).ToList();
        var paymentsQuery = _dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && saleIds.Contains(p.ReferenceId));
        if (from.HasValue) paymentsQuery = paymentsQuery.Where(p => p.CreatedAt >= from);
        if (to.HasValue) paymentsQuery = paymentsQuery.Where(p => p.CreatedAt <= to);

        var payments = await paymentsQuery.OrderBy(p => p.CreatedAt).ToListAsync(cancellationToken);
        foreach (var p in payments)
        {
            entries.Add(new StatementEntryDto(p.CreatedAt, "PAYMENT", p.PaymentId, p.Description ?? "Payment", 0, p.Amount, 0));
        }

        // Returns (credits)
        var returnsQuery = _dbContext.SalesReturns.AsNoTracking()
            .Where(r => r.CustomerId == customerId && r.IsDeleted != true);
        if (from.HasValue) returnsQuery = returnsQuery.Where(r => r.CreatedAt >= from);
        if (to.HasValue) returnsQuery = returnsQuery.Where(r => r.CreatedAt <= to);

        var returns = await returnsQuery.OrderBy(r => r.CreatedAt).ToListAsync(cancellationToken);
        foreach (var r in returns)
        {
            entries.Add(new StatementEntryDto(r.CreatedAt, "RETURN", r.SalesReturnId, r.Note ?? "Sales Return", 0, r.TotalAmount ?? 0m, 0));
        }

        // Calculate running balance
        entries = entries.OrderBy(e => e.Date).ToList();
        decimal runningBalance = 0;
        var finalEntries = new List<StatementEntryDto>();
        foreach (var entry in entries)
        {
            runningBalance += entry.Debit - entry.Credit;
            finalEntries.Add(new StatementEntryDto(entry.Date, entry.Type, entry.ReferenceId, entry.Description, entry.Debit, entry.Credit, runningBalance));
        }

        var openingBalance = runningBalance - finalEntries.Sum(e => e.Debit - e.Credit); // should be 0 if we fetched all
        var closingBalance = runningBalance;

        return new CustomerStatementDto(customerId, customer.Name, finalEntries, openingBalance, closingBalance);
    }

    public async Task<int> CreatePaymentAsync(int referenceType, int referenceId, short? method, decimal amount, string? description, int? userId, CancellationToken cancellationToken = default)
    {
        var payment = new Models.Payment
        {
            Type = 1, // INCOMING
            ReferenceType = (short)referenceType,
            ReferenceId = referenceId,
            Method = method,
            UserId = userId,
            Amount = amount,
            Description = description,
            CreatedAt = DateTime.UtcNow,
        };
        _dbContext.Payments.Add(payment);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return payment.PaymentId;
    }

    public async Task<decimal> GetTotalPaidForSaleAsync(int saleId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Payments.AsNoTracking()
            .Where(p => p.ReferenceType == 1 && p.ReferenceId == saleId)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
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
