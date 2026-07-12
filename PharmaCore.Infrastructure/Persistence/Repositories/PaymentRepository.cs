using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using Npgsql;
using PharmaCore.Infrastructure.Utilities;

namespace PharmaCore.Infrastructure.Persistence.Repositories;

public class PaymentRepository(ApplicationDbContext dbContext) : IPaymentRepository
{
    public async Task<Payment> AddAsync(Payment payment, CancellationToken cancellationToken = default)
    {
        var model = new Models.Payment
        {
            Type = (short)payment.Type,
            ReferenceType = (short)payment.ReferenceType,
            ReferenceId = payment.ReferenceId,
            Method = payment.Method.HasValue ? (short)payment.Method.Value : null,
            UserId = payment.UserId,
            Amount = payment.Amount,
            Description = payment.Description,
            CreatedAt = DateTimeHelper.GetCurrentTimestamp(),
        };

        dbContext.Payments.Add(model);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Map(model);
    }

    public async Task<decimal> GetTotalAmountByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        return  await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;

       
    }

    
    public async Task<IEnumerable<Payment>> ListAsync(CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.IsDeleted != true)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
        return models.Select(Map).ToList();
    }

    public async Task<PagedResult<Payment>> ListPagedAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default)
    {
        var filtered = ApplyFilters(
            dbContext.Payments.AsNoTracking().Include(s => s.User),
            query);
        
        var total = await filtered.CountAsync(cancellationToken);

        var models = await filtered
            .OrderByDescending(s => s.CreatedAt)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);
        
        return new PagedResult<Payment>(
            models.Select(Map).ToList(),
            total,
            query.Page,
            query.Limit);
        
        
    }

    public async Task<PaymentsOverviewDto> GetOverviewAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default)
    {
        var (sql, sqlParams) = BuildAggregateQuery(query);

        var summary = await dbContext.Database
            .SqlQueryRaw<PaymentAggregateRow>(sql, sqlParams)
            .FirstAsync(cancellationToken);

        var pageQuery = ApplyFilters(
            dbContext.Payments.AsNoTracking()
                .Include(p => p.User),
            query);

        var pageModels = await pageQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        var items = await EnrichOverviewItemsAsync(pageModels, cancellationToken);

        return new PaymentsOverviewDto(
            new PaymentOverviewSummaryDto(
                summary.TotalIn,
                summary.TotalOut,
                summary.TotalIn - summary.TotalOut,
                new PaymentOverviewMethodSummaryDto(summary.CashIn, summary.CashOut, summary.CashIn - summary.CashOut),
                new PaymentOverviewMethodSummaryDto(summary.CardIn, summary.CardOut, summary.CardIn - summary.CardOut),
                new PaymentOverviewReferenceSummaryDto(summary.SaleTotal, summary.PurchaseTotal, summary.ExpenseTotal, summary.SalesReturnTotal, summary.PurchaseReturnTotal)),
            items,
            new PaymentOverviewPaginationDto(summary.Total, query.Page, query.Limit));
    }


    public async Task<Payment?> GetByIdAsync(int paymentId, CancellationToken cancellationToken = default)
    {
        var model = await dbContext.Payments
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.PaymentId == paymentId && p.IsDeleted != true, cancellationToken);

        return model is null ? null : Map(model);
    }

    
    public async Task<(IReadOnlyList<Payment> Payments, decimal Total)> GetByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        var payments = models.Select(Map).ToList();
        return (payments, payments.Sum(p => p.Amount));
    }

    
    public async Task<IEnumerable<Payment>> GetByReferencesAsync(
        PaymentReferenceType referenceType,
        IEnumerable<int> referenceIds,
        CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)referenceType && referenceIds.Contains(p.ReferenceId) && p.IsDeleted != true)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return models.Select(Map).ToList();
    }

    public async Task<bool> SoftDeleteByReferenceAsync(
        PaymentReferenceType referenceType,
        int referenceId,
        CancellationToken cancellationToken = default)
    {
        var models = await dbContext.Payments
            .Where(p => p.ReferenceType == (short)referenceType && p.ReferenceId == referenceId && p.IsDeleted != true)
            .ToListAsync(cancellationToken);

        if (models.Count == 0)
            return false;

        foreach (var model in models)
        {
            model.IsDeleted = true;
            model.DeletedAt = DateTimeHelper.GetCurrentTimestamp();
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<decimal> GetTotalRefundedBySaleIdAsync(int saleId, CancellationToken cancellationToken = default)
    {
        var returnIds = await dbContext.SalesReturns
            .AsNoTracking()
            .Where(r => r.SaleId == saleId && r.Status == 2 && r.IsDeleted != true)
            .Select(r => r.SalesReturnId)
            .ToListAsync(cancellationToken);

        if (returnIds.Count == 0)
            return 0m;

        return await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)PaymentReferenceType.SALES_RETURN
                && returnIds.Contains(p.ReferenceId)
                && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    public async Task<decimal> GetTotalRefundedByPurchaseIdAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        var returnIds = await dbContext.PurchaseReturns
            .AsNoTracking()
            .Where(r => r.PurchaseId == purchaseId && r.Status == 2 && r.IsDeleted != true)
            .Select(r => r.PurchaseReturnId)
            .ToListAsync(cancellationToken);

        if (returnIds.Count == 0)
            return 0m;

        return await dbContext.Payments
            .AsNoTracking()
            .Where(p => p.ReferenceType == (short)PaymentReferenceType.PURCHASE_RETURN
                        && returnIds.Contains(p.ReferenceId)
                        && p.IsDeleted != true)
            .SumAsync(p => (decimal?)p.Amount, cancellationToken) ?? 0m;
    }

    private static Payment Map(Models.Payment model)
    {
        return Payment.Rehydrate(
            model.PaymentId,
            (PaymentType)model.Type,
            (PaymentReferenceType)model.ReferenceType,
            model.ReferenceId,
            model.Method.HasValue ? (PaymentMethod?)model.Method.Value : null,
            model.UserId
            ,model.User?.UserName,
            model.Amount,
            model.Description,
            model.CreatedAt,
            model.IsDeleted,
            model.DeletedAt);
    }

    private static IQueryable<Models.Payment> ApplyFilters(IQueryable<Models.Payment> queryable, ListPaymentsQuery query)
    {
        var filtered = queryable.Where(p => p.IsDeleted != true);

        if (query.Type.HasValue)
            filtered = filtered.Where(p => p.Type == (short)query.Type.Value);

        if (query.Method.HasValue)
            filtered = filtered.Where(p => p.Method == (short)query.Method.Value);

        if (query.ReferenceType.HasValue)
            filtered = filtered.Where(p => p.ReferenceType == (short)query.ReferenceType.Value);

        if (query.From.HasValue)
            filtered = filtered.Where(p => p.CreatedAt >= query.From.Value);

        if (query.To.HasValue)
            filtered = filtered.Where(p => p.CreatedAt <= query.To.Value);

        return filtered;
    }

    private async Task<IReadOnlyList<PaymentOverviewItemDto>> EnrichOverviewItemsAsync(
        IReadOnlyList<Models.Payment> payments,
        CancellationToken cancellationToken)
    {
        var saleIds = GetReferenceIds(payments, PaymentReferenceType.SALE);
        var purchaseIds = GetReferenceIds(payments, PaymentReferenceType.PURCHASE);
        var expenseIds = GetReferenceIds(payments, PaymentReferenceType.EXPENSE);
        var salesReturnIds = GetReferenceIds(payments, PaymentReferenceType.SALES_RETURN);
        var purchaseReturnIds = GetReferenceIds(payments, PaymentReferenceType.PURCHASE_RETURN);

        var sales = await dbContext.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Where(s => saleIds.Contains(s.SaleId))
            .Select(s => new
            {
                s.SaleId,
                CustomerName = s.Customer != null ? s.Customer.Name : null,
                TotalAmount = s.TotalAmount ?? 0m
            })
            .ToDictionaryAsync(s => s.SaleId, cancellationToken);

        var purchases = await dbContext.Purchases
            .AsNoTracking()
            .Include(p => p.Supplier)
            .Where(p => purchaseIds.Contains(p.PurchaseId))
            .Select(p => new
            {
                p.PurchaseId,
                p.InvoiceNumber,
                SupplierName = p.Supplier != null ? p.Supplier.Name : null,
                TotalAmount = p.TotalAmount ?? 0m
            })
            .ToDictionaryAsync(p => p.PurchaseId, cancellationToken);

        var expenses = await dbContext.Expenses
            .AsNoTracking()
            .Where(e => expenseIds.Contains(e.ExpenseId))
            .Select(e => new
            {
                e.ExpenseId,
                e.Description,
                TotalAmount = e.Amount ?? 0m
            })
            .ToDictionaryAsync(e => e.ExpenseId, cancellationToken);

        var salesReturns = await dbContext.SalesReturns
            .AsNoTracking()
            .Include(r => r.Customer)
            .Where(r => salesReturnIds.Contains(r.SalesReturnId))
            .Select(r => new
            {
                r.SalesReturnId,
                r.SaleId,
                CustomerName = r.Customer != null ? r.Customer.Name : null,
                TotalAmount = r.TotalAmount ?? 0m
            })
            .ToDictionaryAsync(r => r.SalesReturnId, cancellationToken);

        var purchaseReturns = await dbContext.PurchaseReturns
            .AsNoTracking()
            .Include(r => r.Supplier)
            .Where(r => purchaseReturnIds.Contains(r.PurchaseReturnId))
            .Select(r => new
            {
                r.PurchaseReturnId,
                r.PurchaseId,
                SupplierName = r.Supplier != null ? r.Supplier.Name : null,
                TotalAmount = r.TotalAmount ?? 0m
            })
            .ToDictionaryAsync(r => r.PurchaseReturnId, cancellationToken);

        return payments.Select(payment =>
        {
            var referenceType = (PaymentReferenceType)payment.ReferenceType;
            string referenceLabel;
            string? partyName = null;
            decimal? referenceTotal = null;
            int? parentReferenceId = null;

            switch (referenceType)
            {
                case PaymentReferenceType.SALE when sales.TryGetValue(payment.ReferenceId, out var sale):
                    referenceLabel = $"Sale #{sale.SaleId}";
                    partyName = sale.CustomerName;
                    referenceTotal = sale.TotalAmount;
                    break;
                case PaymentReferenceType.PURCHASE when purchases.TryGetValue(payment.ReferenceId, out var purchase):
                    referenceLabel = string.IsNullOrWhiteSpace(purchase.InvoiceNumber)
                        ? $"Purchase #{purchase.PurchaseId}"
                        : $"Purchase #{purchase.PurchaseId} - {purchase.InvoiceNumber}";
                    partyName = purchase.SupplierName;
                    referenceTotal = purchase.TotalAmount;
                    break;
                case PaymentReferenceType.EXPENSE when expenses.TryGetValue(payment.ReferenceId, out var expense):
                    referenceLabel = string.IsNullOrWhiteSpace(expense.Description)
                        ? $"Expense #{expense.ExpenseId}"
                        : expense.Description;
                    referenceTotal = expense.TotalAmount;
                    break;
                case PaymentReferenceType.SALES_RETURN when salesReturns.TryGetValue(payment.ReferenceId, out var salesReturn):
                    referenceLabel = $"Sales return #{salesReturn.SalesReturnId}";
                    partyName = salesReturn.CustomerName;
                    referenceTotal = salesReturn.TotalAmount;
                    parentReferenceId = salesReturn.SaleId;
                    break;
                case PaymentReferenceType.PURCHASE_RETURN when purchaseReturns.TryGetValue(payment.ReferenceId, out var purchaseReturn):
                    referenceLabel = $"Purchase return #{purchaseReturn.PurchaseReturnId}";
                    partyName = purchaseReturn.SupplierName;
                    referenceTotal = purchaseReturn.TotalAmount;
                    parentReferenceId = purchaseReturn.PurchaseId;
                    break;
                default:
                    referenceLabel = $"{referenceType} #{payment.ReferenceId}";
                    break;
            }

            return new PaymentOverviewItemDto(
                payment.PaymentId,
                (PaymentType)payment.Type,
                referenceType,
                payment.ReferenceId,
                parentReferenceId,
                payment.Method.HasValue ? (PaymentMethod)payment.Method.Value : null,
                payment.UserId,
                payment.User?.UserName,
                payment.Amount,
                payment.Description,
                payment.CreatedAt,
                referenceLabel,
                partyName,
                referenceTotal);
        }).ToList();
    }

    private static IReadOnlyList<int> GetReferenceIds(IReadOnlyList<Models.Payment> payments, PaymentReferenceType referenceType)
    {
        return payments
            .Where(p => p.ReferenceType == (short)referenceType)
            .Select(p => p.ReferenceId)
            .Distinct()
            .ToList();
    }

    private sealed class PaymentAggregateRow
    {
        public int Total { get; init; }
        public decimal TotalIn { get; init; }
        public decimal TotalOut { get; init; }
        public decimal CashIn { get; init; }
        public decimal CashOut { get; init; }
        public decimal CardIn { get; init; }
        public decimal CardOut { get; init; }
        public decimal SaleTotal { get; init; }
        public decimal PurchaseTotal { get; init; }
        public decimal ExpenseTotal { get; init; }
        public decimal SalesReturnTotal { get; init; }
        public decimal PurchaseReturnTotal { get; init; }
    }

    private static (string Sql, NpgsqlParameter[] Params) BuildAggregateQuery(ListPaymentsQuery query)
    {
        var parameters = new List<NpgsqlParameter>();
        var conditions = new List<string> { "p.is_deleted <> true" };

        if (query.Type.HasValue)
        {
            conditions.Add("p.type = @type");
            parameters.Add(new("@type", (short)query.Type.Value));
        }

        if (query.Method.HasValue)
        {
            conditions.Add("p.method = @method");
            parameters.Add(new("@method", (short)query.Method.Value));
        }

        if (query.ReferenceType.HasValue)
        {
            conditions.Add("p.reference_type = @referenceType");
            parameters.Add(new("@referenceType", (short)query.ReferenceType.Value));
        }

        if (query.From.HasValue)
        {
            conditions.Add("p.created_at >= @from");
            parameters.Add(new("@from", query.From.Value));
        }

        if (query.To.HasValue)
        {
            conditions.Add("p.created_at <= @to");
            parameters.Add(new("@to", query.To.Value));
        }

        var whereClause = string.Join(" AND ", conditions);

        var sql = $"""
            SELECT
                COUNT(*)::int AS Total,
                COALESCE(SUM(CASE WHEN p.type = 1 THEN p.amount ELSE 0 END), 0) AS TotalIn,
                COALESCE(SUM(CASE WHEN p.type = 2 THEN p.amount ELSE 0 END), 0) AS TotalOut,
                COALESCE(SUM(CASE WHEN p.method = 1 AND p.type = 1 THEN p.amount ELSE 0 END), 0) AS CashIn,
                COALESCE(SUM(CASE WHEN p.method = 1 AND p.type = 2 THEN p.amount ELSE 0 END), 0) AS CashOut,
                COALESCE(SUM(CASE WHEN p.method = 2 AND p.type = 1 THEN p.amount ELSE 0 END), 0) AS CardIn,
                COALESCE(SUM(CASE WHEN p.method = 2 AND p.type = 2 THEN p.amount ELSE 0 END), 0) AS CardOut,
                COALESCE(SUM(CASE WHEN p.reference_type = 1 THEN p.amount ELSE 0 END), 0) AS SaleTotal,
                COALESCE(SUM(CASE WHEN p.reference_type = 2 THEN p.amount ELSE 0 END), 0) AS PurchaseTotal,
                COALESCE(SUM(CASE WHEN p.reference_type = 3 THEN p.amount ELSE 0 END), 0) AS ExpenseTotal,
                COALESCE(SUM(CASE WHEN p.reference_type = 4 THEN p.amount ELSE 0 END), 0) AS SalesReturnTotal,
                COALESCE(SUM(CASE WHEN p.reference_type = 5 THEN p.amount ELSE 0 END), 0) AS PurchaseReturnTotal
            FROM payments AS p
            WHERE {whereClause}
            """;

        return (sql, [.. parameters]);
    }
}
