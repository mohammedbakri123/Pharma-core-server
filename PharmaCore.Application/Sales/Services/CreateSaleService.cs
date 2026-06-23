using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class CreateSaleService(
    ISaleRepository saleRepository,
    ICustomerRepository customerRepository,
    ILogger<CreateSaleService> logger)
    : ICreateSaleService
{
    public async Task<ServiceResult<SaleDto>> ExecuteAsync(CreateSaleCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.CustomerId.HasValue)
            {
                var customer = await customerRepository.GetByIdAsync(command.CustomerId.Value, cancellationToken);
                if (customer is null)
                    return ServiceResult<SaleDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");
            }
            
            var sale = Sale.Create(command.UserId, command.CustomerId, command.Note);
            var created = await saleRepository.AddAsync(sale, cancellationToken);

            return ServiceResult<SaleDto>.Ok(SaleMappings.MapSale(created));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating sale");
            string errMesage = $"Error creating sale: {e.Message}, {e.InnerException} ,{e.StackTrace}";
            return ServiceResult<SaleDto>.Fail(ServiceErrorType.ServerError, errMesage);
        }
    }
}