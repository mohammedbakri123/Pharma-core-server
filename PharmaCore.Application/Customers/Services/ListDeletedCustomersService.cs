using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class ListDeletedCustomersService(ICustomerRepository customerRepository, ILogger<ListDeletedCustomersService> logger)
    : IListDeletedCustomersService
{
    public async Task<ServiceResult<PagedResult<CustomerDto>>> ExecuteAsync(ListDeletedCustomersQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var customers = await customerRepository.ListDeletedAsync(cancellationToken);

            var filtered = customers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLowerInvariant();
                filtered = filtered.Where(c =>
                    c.Name.ToLowerInvariant().Contains(search) ||
                    (c.PhoneNumber != null && c.PhoneNumber.ToLowerInvariant().Contains(search)));
            }

            var total = filtered.Count();
            var items = filtered
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit).AsEnumerable()
                .Select(MapToDto)
                .ToList();

            return ServiceResult<PagedResult<CustomerDto>>.Ok(
                new PagedResult<CustomerDto>(items, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing deleted customers");
            return ServiceResult<PagedResult<CustomerDto>>.Fail(ServiceErrorType.ServerError, $"Error listing deleted customers: {e.Message}");
        }
    }

    private static CustomerDto MapToDto(Domain.Entities.Customer c) =>
        new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt);
}
