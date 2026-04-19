using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class ListSalesService : IListSalesService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<ListSalesService> _logger;

    public ListSalesService(ISaleRepository saleRepository, ILogger<ListSalesService> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<SaleListItemDto>>> ExecuteAsync(ListSalesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<SaleListItemDto>>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            if (query.From.HasValue && query.To.HasValue && query.From > query.To)
                return ServiceResult<PagedResult<SaleListItemDto>>.Fail(ServiceErrorType.Validation, "From date cannot be later than to date.");

            var sales = await _saleRepository.ListDetailsAsync(query.Page, query.Limit, query.Status, query.UserId, query.CustomerId, query.From, query.To, cancellationToken);
            return ServiceResult<PagedResult<SaleListItemDto>>.Ok(sales);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error listing sales");
            return ServiceResult<PagedResult<SaleListItemDto>>.Fail(ServiceErrorType.ServerError, $"Error listing sales: {e.Message}, {e.InnerException}, {e.Source}, {e.StackTrace}");
        }
    }
}