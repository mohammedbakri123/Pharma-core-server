using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class ListCustomersService(ICustomerRepository customerRepository, ILogger<ListCustomersService> logger)
    : IListCustomersService
{
    public async Task<ServiceResult<PagedResult<CustomerDto>>> ExecuteAsync(ListCustomersQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await customerRepository.ListAsync(query.Search,query.Page,query.Limit,cancellationToken);

            return ServiceResult<PagedResult<CustomerDto>>
                .Ok(MapToDto(result));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing customers");
            return ServiceResult<PagedResult<CustomerDto>>.Fail(ServiceErrorType.ServerError, $"Error listing customers: {e.Message}");
        }
    }

    private static PagedResult<CustomerDto> MapToDto(PagedResult<Customer> result)
    {
        var items = result.Items
            .Select(c => new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt))
            .ToList();

        return new PagedResult<CustomerDto>(items, result.Total, result.Page, result.Limit);
    }}
