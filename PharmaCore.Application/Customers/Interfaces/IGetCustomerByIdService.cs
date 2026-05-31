using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IGetCustomerByIdService
{
    Task<ServiceResult<CustomerDto>> ExecuteAsync(GetCustomerByIdQuery query, CancellationToken cancellationToken = default);
}
