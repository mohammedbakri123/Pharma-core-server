using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Interfaces;

public interface IListPaymentsService
{
    Task<ServiceResult<PagedResult<PaymentDto>>> ExecuteAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default);
}
