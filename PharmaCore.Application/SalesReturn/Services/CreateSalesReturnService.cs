using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class CreateSalesReturnService(
    ISalesReturnRepository salesReturnRepository,
    ISaleRepository saleRepository,
    ILogger<CreateSalesReturnService> logger)
    : ICreateSalesReturnService
{
    public async Task<ServiceResult<SalesReturnDto>> ExecuteAsync(CreateSalesReturnCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await saleRepository.GetByIdAsync(command.SaleId, cancellationToken);
            if (sale is null)
                return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.NotFound, "Sale not found.");

            if (sale.Status != SaleStatus.COMPLETED)
                return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.Validation, "Can only create returns for completed sales.");

            var salesReturn = Domain.Entities.SalesReturn.Create(
                command.SaleId,
                command.CustomerId ?? sale.CustomerId,
                command.UserId,
                command.Note);

            var created = await salesReturnRepository.AddAsync(salesReturn, cancellationToken);

            logger.LogInformation("Created sales return {SalesReturnId}", created.SalesReturnId);

            return ServiceResult<SalesReturnDto>.Ok(new SalesReturnDto(
                created.SalesReturnId,
                created.SaleId,
                created.CustomerId,
                created.UserId,
                created.Status,
                created.TotalAmount,
                created.Note,
                created.CreatedAt));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating sales return");
            return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.ServerError, $"Error creating sales return: {e.Message}");
        }
    }
}
