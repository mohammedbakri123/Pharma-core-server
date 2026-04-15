using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class ListCustomersService : IListCustomersService
{
    private readonly ICustomerRepository _customerRepository;

    public ListCustomersService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ServiceResult<PagedResult<CustomerDto>>> ExecuteAsync(ListCustomersQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var limit = query.Limit <= 0 ? 20 : query.Limit;

        var customers = await _customerRepository.ListAsync(page, limit, query.Search, cancellationToken);

        var items = customers.Items.Select(MapToDto).ToList();

        return ServiceResult<PagedResult<CustomerDto>>.Ok(
            new PagedResult<CustomerDto>(items, customers.Total, page, limit));
    }

    private static CustomerDto MapToDto(Domain.Entities.Customer c) =>
        new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt);
}
