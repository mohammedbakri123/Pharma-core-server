using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Interfaces;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Entities;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Services;

public class CreateSupplierService(ISupplierRepository supplierRepository, ILogger<CreateSupplierService> logger)
    : ICreateSupplierService
{
    public async Task<ServiceResult<SupplierDto>> ExecuteAsync(CreateSupplierCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
            {
                return ServiceResult<SupplierDto>.Fail(ServiceErrorType.Validation, "Supplier name is required.");
            }

            var exists = await supplierRepository.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken);
            if (exists)
            {
                return ServiceResult<SupplierDto>.Fail(ServiceErrorType.Duplicate, "A supplier with this name already exists.");
            }

            var supplier = Supplier.Create(command.Name, command.PhoneNumber, command.Address);
            var created = await supplierRepository.AddAsync(supplier, cancellationToken);

            logger.LogInformation("Supplier '{Name}' created successfully with ID {SupplierId}", created.Name, created.SupplierId);

            return ServiceResult<SupplierDto>.Ok(
                new SupplierDto(created.SupplierId, created.Name, created.PhoneNumber, created.Address, created.CreatedAt));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error creating supplier");
            string errMessage = $"Error creating supplier, {e.Message}";
            return ServiceResult<SupplierDto>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
