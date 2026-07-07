using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class GetSalesReturnByIdService(
    ISalesReturnRepository salesReturnRepository,
    ILogger<GetSalesReturnByIdService> logger)
    : IGetSalesReturnByIdService
{
    public async Task<ServiceResult<SalesReturnDetailsDto>> ExecuteAsync(GetSalesReturnByIdQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = await salesReturnRepository.GetDetailsAsync(query.SalesReturnId, cancellationToken);
            
            if (salesReturn is null)
                return ServiceResult<SalesReturnDetailsDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");
            if (salesReturn.SaleId != query.SaleId)
                return ServiceResult<SalesReturnDetailsDto>.Fail(ServiceErrorType.NotFound, "Sales return not found.");

            var dto = new SalesReturnDetailsDto(
                salesReturn.SalesReturnId,
                salesReturn.SaleId,
                
                salesReturn.CustomerId,
                salesReturn.CustomerName,
                salesReturn.UserId,
                salesReturn.UserName,
                salesReturn.Status,
                salesReturn.TotalAmount,
                salesReturn.Note,
                salesReturn.CreatedAt,
                salesReturn.Items.Select(i => new SalesReturnItemDetailsDto(
                    i.SalesReturnItemId,
                    i.SaleItemId,
                    i.BatchId,
                    i.BatchNumber,
                    i.Quantity,
                    i.UnitPrice,
                    i.TotalPrice,
                    i.MedicineName)).ToList());

            return ServiceResult<SalesReturnDetailsDto>.Ok(dto);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting sales return {SalesReturnId}", query.SalesReturnId);
            return ServiceResult<SalesReturnDetailsDto>.Fail(ServiceErrorType.ServerError, $"Error: {e.Message}");
        }
    }
}