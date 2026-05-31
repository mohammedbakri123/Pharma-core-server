using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface ICreateCustomerService
{
    Task<ServiceResult<CustomerDto>> ExecuteAsync(CreateCustomerCommand command, CancellationToken cancellationToken = default);
}
