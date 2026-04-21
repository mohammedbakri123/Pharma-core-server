using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class GetSalesReturnByIdService : IGetSalesReturnByIdService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly ILogger<GetSalesReturnByIdService> _logger;

    public GetSalesReturnByIdService(ISalesReturnRepository salesReturnRepository, ILogger<GetSalesReturnByIdService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SalesReturnDetailsDto>> ExecuteAsync(GetSalesReturnByIdQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await _salesReturnRepository.GetDetailsAsync(query.SalesReturnId, cancellationToken);
            
            if (salesReturn is null)
                return ServiceResult<SalesReturnDetailsDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            return ServiceResult<SalesReturnDetailsDto>.Ok(salesReturn);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting sales return {SalesReturnId}", query.SalesReturnId);
            return ServiceResult<SalesReturnDetailsDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}