using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class ListSalesService(ISaleRepository saleRepository, ILogger<ListSalesService> logger)
    : IListSalesService
{
    public async Task<ServiceResult<PagedResult<SaleListItemDto>>> ExecuteAsync(ListSalesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<SaleListItemDto>>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            if (query is { From: not null, To: not null } && query.From > query.To)
                return ServiceResult<PagedResult<SaleListItemDto>>.Fail(ServiceErrorType.Validation, "From date cannot be later than to date.");

            var result = await saleRepository.ListPagedAsync(
                query.CustomerId,
                query.UserId,
                query.Status,
                query.From,
                query.To,
                query.Page,
                query.Limit,
                cancellationToken);

            return ServiceResult<PagedResult<SaleListItemDto>>.Ok(MapToDto(result));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing sales");
            return ServiceResult<PagedResult<SaleListItemDto>>.Fail(ServiceErrorType.ServerError, $"Error listing sales: {e.Message}");
        }
    }

    private static PagedResult<SaleListItemDto> MapToDto(PagedResult<Sale> result)
    {
        var items = result.Items
            .Select(s => new SaleListItemDto(
                s.SaleId,
                s.UserId,
                null,
                s.CustomerId,
                null,
                s.Status == SaleStatus.DRAFT ? "Draft" : (s.Status == SaleStatus.COMPLETED ? "completed" : "canceled"),
                s.TotalAmount,
                s.Discount,
                s.CreatedAt,
                s.Note))
            .ToList();

        return new PagedResult<SaleListItemDto>(items, result.Total, result.Page, result.Limit);
    }
}
