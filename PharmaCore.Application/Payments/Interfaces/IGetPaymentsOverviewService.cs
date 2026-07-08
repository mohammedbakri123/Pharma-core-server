using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Interfaces;

public interface IGetPaymentsOverviewService
{
    Task<ServiceResult<PaymentsOverviewDto>> ExecuteAsync(
        ListPaymentsQuery query,
        CancellationToken cancellationToken = default);
}
