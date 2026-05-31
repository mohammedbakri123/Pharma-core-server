using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class AddSaleItemService(
    ISaleRepository saleRepository,
    IMedicineRepository medicineRepository,
    IBatchRepository batchRepository,
    ILogger<AddSaleItemService> logger)
    : IAddSaleItemService
{
    public async Task<ServiceResult<SaleItemDto>> ExecuteAsync(AddSaleItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
            if (sale is null || sale.Status != SaleStatus.DRAFT)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Sale not found or not a draft.");

            if (command.Quantity <= 0)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, "Quantity must be greater than zero.");

            var medicine = await medicineRepository.GetByIdAsync(command.MedicineId, cancellationToken);
            if (medicine is null)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.NotFound, "Medicine not found.");

            var batch = (await batchRepository.ListAvailableByMedicineAsync(command.MedicineId, cancellationToken))
                .FirstOrDefault(b => b.QuantityRemaining >= command.Quantity);

            if (batch is null)
                return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.Validation, "Insufficient stock.");

            var unitPrice = command.UnitPrice ?? batch.SellPrice;
            var item = SaleItem.Create(command.SaleId, command.MedicineId, batch.BatchId, command.Quantity, unitPrice, batch.PurchasePrice);
            var created = await saleRepository.AddItemAsync(item, cancellationToken);
            await saleRepository.UpdateTotalAmountAsync(command.SaleId, cancellationToken);

            return ServiceResult<SaleItemDto>.Ok(SaleMappings.MapItem(created));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding sale item to sale {SaleId}", command.SaleId);
            string errMesage = $"nigaa Error adding sale item: {e.Message}, {e.InnerException}, {e.StackTrace}";
            return ServiceResult<SaleItemDto>.Fail(ServiceErrorType.ServerError, errMesage);
        }
    }
}