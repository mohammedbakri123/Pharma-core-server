using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Application.Purchases.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Purchases.Services;

public class GetPurchaseItemsService : IGetPurchaseItemsService
{
    private readonly IPurchaseRepository _purchaseRepository;
    private readonly ILogger<GetPurchaseItemsService> _logger;

    public GetPurchaseItemsService(
        IPurchaseRepository purchaseRepository,
        ILogger<GetPurchaseItemsService> logger)
    {
        _purchaseRepository = purchaseRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<IReadOnlyList<PurchaseItemDto>>> ExecuteAsync(int purchaseId, CancellationToken cancellationToken = default)
    {
        try
        {
            var items = await _purchaseRepository.GetItemsByPurchaseIdAsync(purchaseId, cancellationToken);

            if (!items.Any())
                return ServiceResult<IReadOnlyList<PurchaseItemDto>>.Fail(ServiceErrorType.NotFound, "No items found for this purchase.");

            var itemDtos = items.Select(i => new PurchaseItemDto(
                i.PurchaseItemId,
                i.MedicineId,
                null,
                i.BatchId,
                null,
                i.Quantity,
                i.PurchasePrice,
                i.SellPrice,
                i.TotalPrice,
                i.ExpireDate)).ToList();

            return ServiceResult<IReadOnlyList<PurchaseItemDto>>.Ok(itemDtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error getting items for purchase {PurchaseId}", purchaseId);
            return ServiceResult<IReadOnlyList<PurchaseItemDto>>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
