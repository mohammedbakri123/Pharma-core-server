using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Interfaces;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Services;

public class UpdateSupplierService(ISupplierRepository supplierRepository, ILogger<UpdateSupplierService> logger)
    : IUpdateSupplierService
{
    public async Task<ServiceResult<SupplierDto>> ExecuteAsync(UpdateSupplierCommand command,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var supplier = await supplierRepository.GetByIdAsync(command.SupplierId, cancellationToken);

            if (supplier is null)
            {
                return ServiceResult<SupplierDto>.Fail(ServiceErrorType.NotFound, $"Supplier with ID {command.SupplierId} not found.");
            }

            if (!string.IsNullOrWhiteSpace(command.Name))
            {
                var exists = await supplierRepository.ExistsByNameAsync(command.Name, command.SupplierId, cancellationToken);
                if (exists)
                {
                    return ServiceResult<SupplierDto>.Fail(ServiceErrorType.Duplicate, "A supplier with this name already exists.");
                }
            }

            supplier.Update(command.Name, command.PhoneNumber, command.Address);
            var updated = await supplierRepository.UpdateAsync(supplier, cancellationToken);

            logger.LogInformation("Supplier '{Name}' updated successfully", updated.Name);

            return ServiceResult<SupplierDto>.Ok(
                new SupplierDto(updated.SupplierId, updated.Name, updated.PhoneNumber, updated.Address, updated.CreatedAt));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error updating supplier {SupplierId}", command.SupplierId);
            string errMessage = $"Error updating supplier, {e.Message}";
            return ServiceResult<SupplierDto>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
