using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerByIdService(ICustomerRepository customerRepository, ILogger<GetCustomerByIdService> logger)
    : IGetCustomerByIdService
{
    public async Task<ServiceResult<CustomerDto>> ExecuteAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var customer = await customerRepository.GetByIdAsync(query.CustomerId, cancellationToken);

            if (customer == null)
                return ServiceResult<CustomerDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");

            return ServiceResult<CustomerDto>.Ok(MapToDto(customer));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting customer {CustomerId}", query.CustomerId);
            return ServiceResult<CustomerDto>.Fail(ServiceErrorType.ServerError, $"Error getting customer: {e.Message}");
        }
    }

    private static CustomerDto MapToDto(Domain.Entities.Customer c) =>
        new CustomerDto(c.CustomerId, c.Name, c.PhoneNumber, c.Address, c.Note, c.CreatedAt);
}
