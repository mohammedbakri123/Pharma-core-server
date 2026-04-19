using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.SalesReturn.Dtos;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.SalesReturn.Services;

public class CreateSalesReturnService : ICreateSalesReturnService
{
    private readonly ISalesReturnRepository _salesReturnRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ILogger<CreateSalesReturnService> _logger;

    public CreateSalesReturnService(
        ISalesReturnRepository salesReturnRepository,
        IBatchRepository batchRepository,
        IStockMovementRepository stockMovementRepository,
        ILogger<CreateSalesReturnService> logger)
    {
        _salesReturnRepository = salesReturnRepository;
        _batchRepository = batchRepository;
        _stockMovementRepository = stockMovementRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SalesReturnDto>> ExecuteAsync(CreateSalesReturnCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var salesReturn = Domain.Entities.SalesReturn.Create(
                command.SaleId,
                command.CustomerId,
                command.UserId,
                command.Note);

            var created = await _salesReturnRepository.AddAsync(salesReturn, cancellationToken);

            _logger.LogInformation("Created sales return {SalesReturnId}", created.SalesReturnId);

            return ServiceResult<SalesReturnDto>.Ok(new SalesReturnDto(
                created.SalesReturnId,
                created.SaleId,
                created.CustomerId,
                created.UserId,
                created.TotalAmount,
                created.Note,
                created.CreatedAt));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating sales return");
            return ServiceResult<SalesReturnDto>.Fail(ServiceErrorType.ServerError, $"Error creating sales return: {e.Message}");
        }
    }
}