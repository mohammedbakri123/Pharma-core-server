using PharmaCore.Application.Customers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Interfaces;

public interface IPayCustomerDebtService
{
    Task<ServiceResult<PayCustomerDebtResult>> ExecuteAsync(PayCustomerDebtCommand command, CancellationToken cancellationToken = default);
}
