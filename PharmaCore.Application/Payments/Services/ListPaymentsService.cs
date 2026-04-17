using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public class ListPaymentsService : IListPaymentsService
{
    private readonly IPaymentQueryRepository _paymentQueryRepository;
    private readonly ILogger<ListPaymentsService> _logger;

    public ListPaymentsService(IPaymentQueryRepository paymentQueryRepository, ILogger<ListPaymentsService> logger)
    {
        _paymentQueryRepository = paymentQueryRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<PaymentDto>>> ExecuteAsync(ListPaymentsQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PagedResult<PaymentDto>>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            if (query.From.HasValue && query.To.HasValue && query.From > query.To)
                return ServiceResult<PagedResult<PaymentDto>>.Fail(ServiceErrorType.Validation, "From date cannot be later than to date.");

            var payments = await _paymentQueryRepository.ListAsync(
                query.Page,
                query.Limit,
                query.Type,
                query.Method,
                query.ReferenceType,
                query.From,
                query.To,
                cancellationToken);

            return ServiceResult<PagedResult<PaymentDto>>.Ok(payments);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error listing payments");
            return ServiceResult<PagedResult<PaymentDto>>.Fail(ServiceErrorType.ServerError, $"Error listing payments: {e.Message}");
        }
    }
}
