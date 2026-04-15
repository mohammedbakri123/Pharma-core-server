using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IUpdateCustomerService
{
    Task<ServiceResult<CustomerDto>> ExecuteAsync(UpdateCustomerCommand command, CancellationToken cancellationToken = default);
}
