using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
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

            var result = await _salesReturnRepository.ListDetailsAsync(
                query.Page,
                query.Limit,
                query.SaleId,
                query.CustomerId,
                query.UserId,
                query.From,
                query.To,
                cancellationToken);

            return ServiceResult<PagedResult<SalesReturnListItemDto>>.Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error listing sales returns");
            return ServiceResult<PagedResult<SalesReturnListItemDto>>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}