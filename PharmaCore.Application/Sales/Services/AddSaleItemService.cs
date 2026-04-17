using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class AddSaleItemService : IAddSaleItemService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IMedicineRepository _medicineRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly ILogger<AddSaleItemService> _logger;

    public AddSaleItemService(ISaleRepository saleRepository, IMedicineRepository medicineRepository, IBatchRepository batchRepository, ILogger<AddSaleItemService> logger)
    {
        _saleRepository = saleRepository;
        _medicineRepository = medicineRepository;
        _batchRepository = batchRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SaleItemDto>> ExecuteAsync(AddSaleItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
            if (sale is null || sale.Status != SaleStatus.DRAFT)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Sale not found or not a draft.");

            if (command.Quantity <= 0)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, "Quantity must be greater than zero.");

            var medicine = await _medicineRepository.GetByIdAsync(command.MedicineId, cancellationToken);
            if (medicine is null)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Medicine not found.");

            var batch = (await _batchRepository.ListAvailableByMedicineAsync(command.MedicineId, cancellationToken))
                .FirstOrDefault(b => b.QuantityRemaining >= command.Quantity);

            if (batch is null)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, "Insufficient stock.");

            var unitPrice = command.UnitPrice ?? batch.SellPrice;
            var item = SaleItem.Create(command.SaleId, command.MedicineId, batch.BatchId, command.Quantity, unitPrice);
            var created = await _saleRepository.AddItemAsync(item, cancellationToken);
            await _saleRepository.UpdateTotalAmountAsync(command.SaleId, cancellationToken);

            return ServiceResult<SaleItemDto>.Ok(SaleMappings.MapItem(created));
        }
        catch (ArgumentException e)
        {
            return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error adding sale item to sale {SaleId}", command.SaleId);
            return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.ServerError, $"Error adding sale item: {e.Message}");
        }
    }
}