using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using  PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class ListSalesReturnService(
    ISalesReturnRepository salesReturnRepository,
    ILogger<ListSalesReturnService> logger)
    : IListSalesReturnService
{
    public async Task<ServiceResult<PagedResult<SalesReturnDto>>> ExecuteAsync(ListSalesReturnQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<SalesReturnDto>>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            var returns = await salesReturnRepository.ListPagedAsync(query, cancellationToken);

            return ServiceResult<PagedResult<SalesReturnDto>>.Ok(map(returns));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing sales returns");
            return ServiceResult<PagedResult<SalesReturnDto>>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }

    private PagedResult<SalesReturnDto> map(PagedResult<Domain.Entities.SalesReturn> result)
    {
        var items = result.Items.Select(r => new SalesReturnDto(r.SalesReturnId,r.SaleId,r.CustomerId,r.UserId,r.UserName,r.Status,r.TotalAmount,r.Note,r.CreatedAt))
            .ToList();
        
        return new PagedResult<SalesReturnDto>(items, result.Total, result.Page, result.Limit);
        
    }
}