using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IHardDeleteCustomerService
{
    Task<ServiceResult<bool>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default);
}