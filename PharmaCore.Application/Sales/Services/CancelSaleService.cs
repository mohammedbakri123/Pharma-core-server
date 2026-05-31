using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class CancelSaleService : ICancelSaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ILogger<CancelSaleService> _logger;

    public CancelSaleService(ISaleRepository saleRepository, ILogger<CancelSaleService> logger)
    {
        _saleRepository = saleRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<bool>> ExecuteAsync(int saleId, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _saleRepository.GetByIdAsync(saleId, cancellationToken);
            if (sale is null)
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Sale not found.");

            if (sale.Status != SaleStatus.DRAFT)
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Only draft sales can be cancelled.");

            sale.Cancel();
            await _saleRepository.UpdateAsync(sale, cancellationToken);
            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error cancelling sale {SaleId}", saleId);
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Error cancelling sale: {e.Message}");
        }
    }
}