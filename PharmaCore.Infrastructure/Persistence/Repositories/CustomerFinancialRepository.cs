using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class CustomerFinancialRepository : ICustomerFinancialRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CustomerFinancialRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
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
}
