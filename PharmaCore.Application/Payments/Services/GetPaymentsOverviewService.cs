using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Dtos;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Payments.Services;

public sealed class GetPaymentsOverviewService(
    IPaymentRepository paymentRepository,
    ILogger<GetPaymentsOverviewService> logger)
    : IGetPaymentsOverviewService
{
    public async Task<ServiceResult<PaymentsOverviewDto>> ExecuteAsync(
        ListPaymentsQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (query.Page <= 0 || query.Limit <= 0)
                return ServiceResult<PaymentsOverviewDto>.Fail(ServiceErrorType.Validation, "Page and limit must be greater than zero.");

            if (query.From.HasValue && query.To.HasValue && query.From > query.To)
                return ServiceResult<PaymentsOverviewDto>.Fail(ServiceErrorType.Validation, "From date cannot be later than to date.");

            var overview = await paymentRepository.GetOverviewAsync(query, cancellationToken);
            return ServiceResult<PaymentsOverviewDto>.Ok(overview);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting payments overview");
            return ServiceResult<PaymentsOverviewDto>.Fail(ServiceErrorType.ServerError, $"Error getting payments overview: {e.Message}");
        }
    }
}
