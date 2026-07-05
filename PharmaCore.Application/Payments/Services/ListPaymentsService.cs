using System.Linq;
using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class ListPaymentsService(IPaymentRepository paymentRepository, ILogger<ListPaymentsService> logger)
    : IListPaymentsService
{
    public async Task<ServiceResult<PagedResult<PaymentDto>>> ExecuteAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<PaymentDto>>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            if (query.From.HasValue && query.To.HasValue && query.From > query.To)
                return ServiceResult<PagedResult<PaymentDto>>.Fail(ServiceErrorType.Validation, "From date cannot be later than to date.");

            var payments = await paymentRepository.ListPagedAsync(query, cancellationToken);

            return ServiceResult<PagedResult<PaymentDto>>.Ok(Map(payments));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing payments");
            return ServiceResult<PagedResult<PaymentDto>>.Fail(ServiceErrorType.ServerError, $"Error listing payments: {e.Message}");
        }
    }

    private PagedResult<PaymentDto> Map(PagedResult<Payment> result)
    {
        var item = result.Items.Select(PaymentMappings.MapToDto).ToList();
        
        return new PagedResult<PaymentDto>(item, result.Total, result.Page, result.Limit);
    }
}
