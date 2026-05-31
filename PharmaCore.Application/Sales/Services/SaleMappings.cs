using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Sales.Services;

public static class SaleMappings
{
    public static SaleDto MapSale(Sale sale) => new(sale.SaleId, sale.UserId, sale.CustomerId, sale.Status, sale.TotalAmount, sale.Discount, sale.CreatedAt, sale.Note);
    public static SaleItemDto MapItem(SaleItem item) => new(item.SaleItemId, item.SaleId, item.MedicineId, item.BatchId, item.Quantity, item.UnitPrice, item.TotalPrice);
}