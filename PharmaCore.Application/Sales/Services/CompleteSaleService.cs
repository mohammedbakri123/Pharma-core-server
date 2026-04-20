using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class CompleteSaleService : ICompleteSaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly IBatchRepository _batchRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly ILogger<CompleteSaleService> _logger;

    public CompleteSaleService(ISaleRepository saleRepository, IBatchRepository batchRepository, IPaymentRepository paymentRepository, IStockMovementRepository stockMovementRepository, ILogger<CompleteSaleService> logger)
    {
        _saleRepository = saleRepository;
        _batchRepository = batchRepository;
        _paymentRepository = paymentRepository;
        _stockMovementRepository = stockMovementRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<CompleteSaleResultDto>> ExecuteAsync(CompleteSaleCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var sale = await _saleRepository.GetByIdWithItemsAsync(command.SaleId, cancellationToken);
            if (sale is null)
                return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.NotFound, "Sale not found.");

            if (sale.Status != SaleStatus.DRAFT)
                return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, "Sale cannot be completed because it is not a draft.");

            if (sale.Items.Count == 0)
                return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, "Sale cannot be completed without items.");

            foreach (var item in sale.Items)
            {
                var batch = await _batchRepository.GetByIdAsync(item.BatchId, cancellationToken);
                if (batch is null || batch.QuantityRemaining < item.Quantity)
                    return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, "Sale cannot be completed due to insufficient stock.");
            }

            var paymentTotal = command.Payments.Sum(p => p.Amount);
            if (paymentTotal < 0)
                return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, "Payment amount cannot be negative.");

            if (paymentTotal > sale.TotalAmount)
                return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, "Paid amount cannot exceed sale total.");

            foreach (var item in sale.Items)
            {
                var affected = await _batchRepository.DecrementBatchStockAsync(item.BatchId, item.Quantity, cancellationToken);
                if (affected <= 0)
                    return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, "Sale cannot be completed due to insufficient stock.");
            }

            var stockMovements = sale.Items
                .Select(item => StockMovement.Create(
                    item.MedicineId,
                    item.BatchId,
                    item.Quantity,
                    StockMovementType.OUT,
                    StockMovementReferenceType.SALE,
                    sale.SaleId))
                .ToList();

            var createdMovements = await _stockMovementRepository.AddRangeAsync(stockMovements, cancellationToken);

            var paymentsCreated = 0;
            foreach (var payment in command.Payments.Where(p => p.Amount > 0))
            {
                var paymentEntity = Payment.Create(
                    PaymentType.INCOMING,
                    PaymentReferenceType.SALE,
                    sale.SaleId,
                    payment.Method,
                    command.UserId,
                    payment.Amount,
                    payment.Description);

                await _paymentRepository.AddAsync(paymentEntity, cancellationToken);
                paymentsCreated++;
            }

            sale.Complete();
            var updatedSale = await _saleRepository.UpdateAsync(sale, cancellationToken);
            var paidAmount = await _paymentRepository.GetTotalAmountByReferenceAsync(PaymentReferenceType.SALE, updatedSale.SaleId, cancellationToken);

            var result = new CompleteSaleResultDto(
                updatedSale.SaleId,
                updatedSale.Status,
                updatedSale.TotalAmount,
                updatedSale.Discount,
                DateTime.UtcNow,
                createdMovements.Count,
                paymentsCreated,
                new SaleBalanceDto(updatedSale.SaleId, updatedSale.TotalAmount, paidAmount, updatedSale.TotalAmount - paidAmount));

            return ServiceResult<CompleteSaleResultDto>.Ok(result);
        }
        catch (ArgumentException e)
        {
            return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (InvalidOperationException e)
        {
            return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.Validation, e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error completing sale {SaleId}", command.SaleId);
            return ServiceResult<CompleteSaleResultDto>.Fail(ServiceErrorType.ServerError, $"Error completing sale: {e.Message}");
        }
    }
}