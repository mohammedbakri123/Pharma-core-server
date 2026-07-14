using System.Linq;
using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
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
        var filteredSummary = ApplyFilters(dbContext.Payments.AsNoTracking(), query);

        var incoming = (short)PaymentType.INCOMING;
        var outgoing = (short)PaymentType.OUTGOING;
        var cash = (short)PaymentMethod.CASH;
        var card = (short)PaymentMethod.CARD;
        var sale = (short)PaymentReferenceType.SALE;
        var purchase = (short)PaymentReferenceType.PURCHASE;
        var expense = (short)PaymentReferenceType.EXPENSE;
        var salesReturn = (short)PaymentReferenceType.SALES_RETURN;
        var purchaseReturn = (short)PaymentReferenceType.PURCHASE_RETURN;

        var summary = await filteredSummary
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Total = g.Count(),
                TotalIn = g.Sum(p => p.Type == incoming ? p.Amount : 0m),
                TotalOut = g.Sum(p => p.Type == outgoing ? p.Amount : 0m),
                CashIn = g.Sum(p => p.Method == cash && p.Type == incoming ? p.Amount : 0m),
                CashOut = g.Sum(p => p.Method == cash && p.Type == outgoing ? p.Amount : 0m),
                CardIn = g.Sum(p => p.Method == card && p.Type == incoming ? p.Amount : 0m),
                CardOut = g.Sum(p => p.Method == card && p.Type == outgoing ? p.Amount : 0m),
                SaleTotal = g.Sum(p => p.ReferenceType == sale ? p.Amount : 0m),
                PurchaseTotal = g.Sum(p => p.ReferenceType == purchase ? p.Amount : 0m),
                ExpenseTotal = g.Sum(p => p.ReferenceType == expense ? p.Amount : 0m),
                SalesReturnTotal = g.Sum(p => p.ReferenceType == salesReturn ? p.Amount : 0m),
                PurchaseReturnTotal = g.Sum(p => p.ReferenceType == purchaseReturn ? p.Amount : 0m)
            })
            .FirstOrDefaultAsync(cancellationToken);

        var pageModels = await ApplyFilters(
                dbContext.Payments.AsNoTracking().Include(p => p.User),
                query)
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.Limit)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        var items = await EnrichOverviewItemsAsync(pageModels, cancellationToken);
        var totalIn = summary?.TotalIn ?? 0m;
        var totalOut = summary?.TotalOut ?? 0m;
        var cashIn = summary?.CashIn ?? 0m;
        var cashOut = summary?.CashOut ?? 0m;
        var cardIn = summary?.CardIn ?? 0m;
        var cardOut = summary?.CardOut ?? 0m;

        return new PaymentsOverviewDto(
            new PaymentOverviewSummaryDto(
                totalIn,
                totalOut,
                totalIn - totalOut,
                new PaymentOverviewMethodSummaryDto(cashIn, cashOut, cashIn - cashOut),
                new PaymentOverviewMethodSummaryDto(cardIn, cardOut, cardIn - cardOut),
                new PaymentOverviewReferenceSummaryDto(
                    summary?.SaleTotal ?? 0m,
                    summary?.PurchaseTotal ?? 0m,
                    summary?.ExpenseTotal ?? 0m,
                    summary?.SalesReturnTotal ?? 0m,
                    summary?.PurchaseReturnTotal ?? 0m)),
            items,
            new PaymentOverviewPaginationDto(summary?.Total ?? 0, query.Page, query.Limit));
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

        var sales = saleIds.Count == 0
            ? new Dictionary<int, SaleOverviewReference>()
            : await dbContext.Sales
                .AsNoTracking()
                .Where(s => saleIds.Contains(s.SaleId))
                .Select(s => new SaleOverviewReference(
                    s.SaleId,
                    s.Customer != null ? s.Customer.Name : null,
                    s.TotalAmount ?? 0m))
                .ToDictionaryAsync(s => s.SaleId, cancellationToken);

        var purchases = purchaseIds.Count == 0
            ? new Dictionary<int, PurchaseOverviewReference>()
            : await dbContext.Purchases
                .AsNoTracking()
                .Where(p => purchaseIds.Contains(p.PurchaseId))
                .Select(p => new PurchaseOverviewReference(
                    p.PurchaseId,
                    p.InvoiceNumber,
                    p.Supplier != null ? p.Supplier.Name : null,
                    p.TotalAmount ?? 0m))
                .ToDictionaryAsync(p => p.PurchaseId, cancellationToken);

        var expenses = expenseIds.Count == 0
            ? new Dictionary<int, ExpenseOverviewReference>()
            : await dbContext.Expenses
                .AsNoTracking()
                .Where(e => expenseIds.Contains(e.ExpenseId))
                .Select(e => new ExpenseOverviewReference(
                    e.ExpenseId,
                    e.Description,
                    e.Amount ?? 0m))
                .ToDictionaryAsync(e => e.ExpenseId, cancellationToken);

        var salesReturns = salesReturnIds.Count == 0
            ? new Dictionary<int, SalesReturnOverviewReference>()
            : await dbContext.SalesReturns
                .AsNoTracking()
                .Where(r => salesReturnIds.Contains(r.SalesReturnId))
                .Select(r => new SalesReturnOverviewReference(
                    r.SalesReturnId,
                    r.SaleId,
                    r.Customer != null ? r.Customer.Name : null,
                    r.TotalAmount ?? 0m))
                .ToDictionaryAsync(r => r.SalesReturnId, cancellationToken);

        var purchaseReturns = purchaseReturnIds.Count == 0
            ? new Dictionary<int, PurchaseReturnOverviewReference>()
            : await dbContext.PurchaseReturns
                .AsNoTracking()
                .Where(r => purchaseReturnIds.Contains(r.PurchaseReturnId))
                .Select(r => new PurchaseReturnOverviewReference(
                    r.PurchaseReturnId,
                    r.PurchaseId,
                    r.Supplier != null ? r.Supplier.Name : null,
                    r.TotalAmount ?? 0m))
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

    private sealed record SaleOverviewReference(int SaleId, string? CustomerName, decimal TotalAmount);

    private sealed record PurchaseOverviewReference(
        int PurchaseId,
        string? InvoiceNumber,
        string? SupplierName,
        decimal TotalAmount);

    private sealed record ExpenseOverviewReference(int ExpenseId, string? Description, decimal TotalAmount);

    private sealed record SalesReturnOverviewReference(
        int SalesReturnId,
        int? SaleId,
        string? CustomerName,
        decimal TotalAmount);

    private sealed record PurchaseReturnOverviewReference(
        int PurchaseReturnId,
        int? PurchaseId,
        string? SupplierName,
        decimal TotalAmount);
}
