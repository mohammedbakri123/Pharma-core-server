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
    public async Task<ServiceResult<IReadOnlyList<SaleItemDto>>> ExecuteAsync(AddSaleItemCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await saleRepository.GetByIdWithItemsAsync(command.SaleId, cancellationToken);
            if (sale is null || sale.Status != SaleStatus.DRAFT)
                return ServiceResult<IReadOnlyList<SaleItemDto>>.Fail(ServiceErrorType.NotFound, "Sale not found or not a draft.");

            if (command.Quantity <= 0)
                return ServiceResult<IReadOnlyList<SaleItemDto>>.Fail(ServiceErrorType.Validation, "Quantity must be greater than zero.");

            var medicine = await medicineRepository.GetByIdAsync(command.MedicineId, cancellationToken);
            if (medicine is null)
                return ServiceResult<IReadOnlyList<SaleItemDto>>.Fail(ServiceErrorType.NotFound, "Medicine not found.");

            var batches = (await batchRepository.ListAvailableByMedicineAsync(command.MedicineId, cancellationToken)).ToList();
            if (batches.Count == 0)
                return ServiceResult<IReadOnlyList<SaleItemDto>>.Fail(ServiceErrorType.Validation, "Insufficient stock.");

            var reservedByBatch = sale.Items
                .Where(i => i.MedicineId == command.MedicineId)
                .GroupBy(i => i.BatchId)
                .ToDictionary(g => g.Key, g => g.Sum(i => i.Quantity));

            var totalAvailable = batches.Sum(b => Math.Max(0, b.QuantityRemaining - reservedByBatch.GetValueOrDefault(b.BatchId, 0)));
            if (totalAvailable < command.Quantity)
                return ServiceResult<IReadOnlyList<SaleItemDto>>.Fail(ServiceErrorType.Validation, "Insufficient stock.");

            var remaining = command.Quantity;
            var createdItems = new List<SaleItemDto>();

            foreach (var batch in batches)
            {
                if (remaining <= 0) break;

                var netAvailable = Math.Max(0, batch.QuantityRemaining - reservedByBatch.GetValueOrDefault(batch.BatchId, 0));
                if (netAvailable <= 0) continue;

                var take = Math.Min(remaining, netAvailable);
                var item = SaleItem.Create(command.SaleId, command.MedicineId, batch.BatchId, take, batch.SellPrice, batch.PurchasePrice);
                var created = await saleRepository.AddItemAsync(item, cancellationToken);
                createdItems.Add(SaleMappings.MapItem(created));
                remaining -= take;
            }

            await saleRepository.UpdateTotalAmountAsync(command.SaleId, cancellationToken);

            return ServiceResult<IReadOnlyList<SaleItemDto>>.Ok(createdItems);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error adding sale item to sale {SaleId}", command.SaleId);
            string errMesage = $"Error adding sale item: {e.Message}, {e.InnerException}, {e.StackTrace}";
            return ServiceResult<IReadOnlyList<SaleItemDto>>.Fail(ServiceErrorType.ServerError, errMesage);
        }
    }
}