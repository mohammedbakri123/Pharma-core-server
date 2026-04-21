using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IRestoreCustomerService
{
    Task<ServiceResult<bool>> ExecuteAsync(RestoreCustomerCommand command, CancellationToken cancellationToken = default);
}
