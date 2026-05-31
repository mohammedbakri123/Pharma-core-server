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

public class ListPaymentsService : IListPaymentsService
{
    private readonly IPaymentRepository _paymentRepository;
    private readonly ILogger<ListPaymentsService> _logger;

    public ListPaymentsService(IPaymentRepository paymentRepository, ILogger<ListPaymentsService> logger)
    {
        _paymentRepository = paymentRepository;
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

            var payments = await _paymentRepository.ListAsync(cancellationToken);

            var filtered = payments.AsEnumerable();

            if (query.Type.HasValue)
                filtered = filtered.Where(p => p.Type == query.Type.Value);

            if (query.Method.HasValue)
                filtered = filtered.Where(p => p.Method == query.Method.Value);

            if (query.ReferenceType.HasValue)
                filtered = filtered.Where(p => p.ReferenceType == query.ReferenceType.Value);

            if (query.From.HasValue)
                filtered = filtered.Where(p => p.CreatedAt >= query.From.Value);

            if (query.To.HasValue)
                filtered = filtered.Where(p => p.CreatedAt <= query.To.Value);

            var total = filtered.Count();
            var items = filtered
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(p => new PaymentDto(
                    p.PaymentId,
                    p.Type,
                    p.ReferenceType,
                    p.ReferenceId,
                    p.Method,
                    p.UserId,
                    null,
                    p.Amount,
                    p.Description,
                    p.CreatedAt))
                .ToList();

            return ServiceResult<PagedResult<PaymentDto>>.Ok(new PagedResult<PaymentDto>(items, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error listing payments");
            return ServiceResult<PagedResult<PaymentDto>>.Fail(ServiceErrorType.ServerError, $"Error listing payments: {e.Message}");
        }
    }
}
