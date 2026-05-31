using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Interfaces;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Services;

public class GetSupplierByIdService(ISupplierRepository supplierRepository, ILogger<GetSupplierByIdService> logger)
    : IGetSupplierByIdService
{
    public async Task<ServiceResult<SupplierDto>> ExecuteAsync(GetSupplierByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var supplier = await supplierRepository.GetByIdAsync(query.SupplierId, cancellationToken);

            if (supplier is null)
            {
                return ServiceResult<SupplierDto>.Fail(ServiceErrorType.NotFound, $"Supplier with ID {query.SupplierId} not found.");
            }

            return ServiceResult<SupplierDto>.Ok(
                new SupplierDto(supplier.SupplierId, supplier.Name, supplier.PhoneNumber, supplier.Address, supplier.CreatedAt));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting supplier by ID {SupplierId}", query.SupplierId);
            string errMessage = $"Error getting supplier, {e.Message}";
            return ServiceResult<SupplierDto>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
