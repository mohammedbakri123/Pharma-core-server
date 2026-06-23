using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class GetSaleByIdService(ISaleRepository saleRepository, ILogger<GetSaleByIdService> logger)
    : IGetSaleByIdService
{
    public async Task<ServiceResult<SaleDetailsDto>> ExecuteAsync(GetSaleByIdQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await saleRepository.GetDetailsAsync(query.SaleId, cancellationToken);
            if (sale is null)
                return ServiceResult<SaleDetailsDto>.Fail(ServiceErrorType.NotFound, "Sale not found.");

            var dto = new SaleDetailsDto(
                sale.SaleId,
                sale.UserId,
                sale.UserName,
                sale.CustomerId,
                sale.CustomerName,
                sale.Status,
                sale.TotalAmount,
                sale.Discount,
                sale.CreatedAt,
                sale.Note,
                sale.Items.Select(i => new SaleItemDetailsDto(
                    i.SaleItemId,
                    i.MedicineId,
                    i.MedicineName,
                    i.BatchId,
                    i.BatchNumber,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice)).ToList());

            return ServiceResult<SaleDetailsDto>.Ok(dto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting sale {SaleId}", query.SaleId);
            string errMessage = $"Error getting sale, {e.Message}, {e.InnerException}, {e.StackTrace}";
            return ServiceResult<SaleDetailsDto>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}