using PharmaCore.Application.Purchases.Dtos;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Purchases.Services;

public static class PurchaseMappings
{
    public static PurchaseDto MapPurchase(Purchase purchase) => new(
        purchase.PurchaseId,
        purchase.SupplierId,
        null,
        purchase.InvoiceNumber,
        purchase.TotalAmount,
        purchase.Status,
        purchase.CreatedAt,
        purchase.Note,
        purchase.Items.Select(MapItem).ToList());

    public static PurchaseItemDto MapItem(PurchaseItem item) => new(
        item.PurchaseItemId,
        item.MedicineId,
        null,
        item.BatchId,
        item.BatchNumber,
        item.Quantity,
        item.PurchasePrice,
        item.SellPrice,
        item.TotalPrice,
        item.ExpireDate);

    public static PurchaseListItemDto MapListItem(Purchase purchase) => new(
        purchase.PurchaseId,
        purchase.SupplierId,
        null,
        purchase.InvoiceNumber,
        purchase.TotalAmount,
        purchase.Status,
        purchase.CreatedAt,
        purchase.Note);
}
