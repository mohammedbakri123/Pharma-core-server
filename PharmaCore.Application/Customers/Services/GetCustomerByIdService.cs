using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerByIdService : IGetCustomerByIdService
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerByIdService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ServiceResult<CustomerDto>> ExecuteAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(query.CustomerId, cancellationToken);

        if (customer == null)
            return ServiceResult<CustomerDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");

        return ServiceResult<CustomerDto>.Ok(MapToDto(customer));
    }

    private static CustomerDto MapToDto(Domain.Entities.Customer c) =>
        new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt);
}
