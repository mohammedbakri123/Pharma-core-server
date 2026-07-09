using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Requests;
using PharmaCore.Application.POS.Dtos;
using PharmaCore.Application.POS.Interfaces;
using PharmaCore.Application.POS.Requests;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Enums;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.POS.Services;

public class PosCheckoutService(
    ICreateSaleService createSaleService,
    IAddSaleItemService addSaleItemService,
    ICompleteSaleService completeSaleService,
    ICreatePaymentService createPaymentService,
    IMedicineRepository medicineRepository,
    ICustomerRepository customerRepository,
    IUserRepository userRepository,
    ILogger<PosCheckoutService> logger) : IPosCheckoutService
{
    public async Task<ServiceResult<PosCheckoutResultDto>> ExecuteAsync(PosCheckoutCommand command, CancellationToken ct)
    {
        try
        {
            var createResult = await createSaleService.ExecuteAsync(
                new CreateSaleCommand(command.UserId, command.CustomerId, command.Note), ct);
            if (!createResult.Success)
                return ServiceResult<PosCheckoutResultDto>.Fail(createResult.Error.Type, createResult.Error.Message);
            var sale = createResult.Data!;

            var allItemDtos = new List<Sales.Dtos.SaleItemDto>();
            foreach (var item in command.Items)
            {
                var addResult = await addSaleItemService.ExecuteAsync(
                    new AddSaleItemCommand(sale.SaleId, item.MedicineId, item.Quantity), ct);
                if (!addResult.Success)
                    return ServiceResult<PosCheckoutResultDto>.Fail(addResult.Error.Type, addResult.Error.Message);
                allItemDtos.AddRange(addResult.Data!);
            }

            var completeResult = await completeSaleService.ExecuteAsync(
                new CompleteSaleCommand(sale.SaleId, command.UserId), ct);
            if (!completeResult.Success)
                return ServiceResult<PosCheckoutResultDto>.Fail(completeResult.Error.Type, completeResult.Error.Message);

            var paymentIds = new List<int>();
            foreach (var payment in command.Payments)
            {
                var paymentResult = await createPaymentService.ExecuteAsync(
                    new CreatePaymentCommand(
                        PaymentReferenceType.SALE, sale.SaleId, payment.Method,
                        payment.Amount, null, command.UserId), ct);
                if (!paymentResult.Success)
                    return ServiceResult<PosCheckoutResultDto>.Fail(paymentResult.Error.Type, paymentResult.Error.Message);
                paymentIds.Add(paymentResult.Data!.PaymentId);
            }

            var medicineIds = allItemDtos.Select(i => i.MedicineId).Distinct().ToList();
            var nameMap = new Dictionary<int, string>();
            foreach (var id in medicineIds)
            {
                var med = await medicineRepository.GetByIdAsync(id, ct);
                nameMap[id] = med?.Name ?? med?.ArabicName ?? $"ID {id}";
            }

            var customerName = command.CustomerId.HasValue
                ? (await customerRepository.GetByIdAsync(command.CustomerId.Value, ct))?.Name
                : null;

            var userName = command.UserId.HasValue
                ? (await userRepository.GetByIdAsync(command.UserId.Value, ct))?.UserName
                : null;

            var subtotal = allItemDtos.Sum(i => i.TotalPrice);
            var total = subtotal - command.Discount;
            var totalPaid = command.Payments.Sum(p => p.Amount);

            var itemDtos = allItemDtos.Select(i => new PosCheckoutItemDto(
                i.MedicineId, nameMap.GetValueOrDefault(i.MedicineId), i.Quantity, i.UnitPrice, i.TotalPrice
            )).ToList();

            var paymentDtos = command.Payments
                .Select(p => new PosCheckoutPaymentDto(p.Method, p.Amount))
                .ToList();

            var result = new PosCheckoutResultDto(
                sale.SaleId,
                paymentIds,
                SaleStatus.COMPLETED,
                subtotal,
                command.Discount,
                total,
                paymentDtos,
                totalPaid,
                Math.Max(0, totalPaid - total),
                itemDtos,
                sale.CreatedAt,
                command.CustomerId,
                customerName,
                userName);

            return ServiceResult<PosCheckoutResultDto>.Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "POS checkout failed");
            return ServiceResult<PosCheckoutResultDto>.Fail(ServiceErrorType.ServerError, $"Checkout failed: {e.Message}");
        }
    }
}
