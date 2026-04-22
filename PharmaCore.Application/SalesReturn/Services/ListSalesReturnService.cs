using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class ListSalesReturnService : IListSalesReturnService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly ILogger<ListSalesReturnService> _logger;

    public ListSalesReturnService(ISalesReturnRepository salesReturnRepository, ILogger<ListSalesReturnService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<SalesReturnListItemDto>>> ExecuteAsync(ListSalesReturnQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<SalesReturnListItemDto>>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            var returns = await _salesReturnRepository.ListDetailsAsync(cancellationToken);
            
            var filtered = returns.AsEnumerable();
            
            if (query.SaleId.HasValue)
                filtered = filtered.Where(r => r.SaleId == query.SaleId.Value);
            
            if (query.CustomerId.HasValue)
                filtered = filtered.Where(r => r.CustomerId == query.CustomerId.Value);
            
            if (query.UserId.HasValue)
                filtered = filtered.Where(r => r.UserId == query.UserId.Value);
            
            if (query.From.HasValue)
                filtered = filtered.Where(r => r.CreatedAt >= query.From.Value);
            
            if (query.To.HasValue)
                filtered = filtered.Where(r => r.CreatedAt <= query.To.Value);
            
            var total = filtered.Count();
            var items = filtered
                .OrderByDescending(r => r.CreatedAt)
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(r => new SalesReturnListItemDto(
                    r.SalesReturnId,
                    r.SaleId,
                    r.SaleId?.ToString(),
                    r.CustomerId,
                    null,
                    r.UserId,
                    null,
                    r.TotalAmount,
                    r.Note,
                    r.CreatedAt))
                .ToList();
            
            return ServiceResult<PagedResult<SalesReturnListItemDto>>.Ok(new PagedResult<SalesReturnListItemDto>(items, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error listing sales returns");
            return ServiceResult<PagedResult<SalesReturnListItemDto>>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}