using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IDeleteCustomerService
{
    Task<ServiceResult<bool>> ExecuteAsync(DeleteCustomerCommand command, CancellationToken cancellationToken = default);
}
