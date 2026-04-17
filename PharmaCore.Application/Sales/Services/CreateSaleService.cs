using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class CreateSaleService : ICreateSaleService
{
    private readonly ISaleRepository _saleRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CreateSaleService> _logger;

    public CreateSaleService(ISaleRepository saleRepository, ICustomerRepository customerRepository, ILogger<CreateSaleService> logger)
    {
        _saleRepository = saleRepository;
        _customerRepository = customerRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<SaleDto>> ExecuteAsync(CreateSaleCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (command.CustomerId.HasValue)
            {
                var customer = await _customerRepository.GetByIdAsync(command.CustomerId.Value, cancellationToken);
                if (customer is null)
                    return ServiceResult<SaleDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");
            }

            if ((command.Discount ?? 0m) < 0)
                return ServiceResult<SaleDto>.Fail(ServiceErrorType.Validation, "Discount cannot be negative.");

            if ((command.Discount ?? 0m) > 0)
                return ServiceResult<SaleDto>.Fail(ServiceErrorType.Validation, "Discount cannot be applied before sale items are added.");

            var sale = Sale.Create(command.UserId, command.CustomerId, command.Note);
            var created = await _saleRepository.AddAsync(sale, cancellationToken);

            return ServiceResult<SaleDto>.Ok(SaleMappings.MapSale(created));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error creating sale");
            return ServiceResult<SaleDto>.Fail(ServiceErrorType.ServerError, $"Error creating sale: {e.Message}");
        }
    }
}