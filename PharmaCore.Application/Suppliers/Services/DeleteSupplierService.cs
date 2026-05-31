using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Suppliers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Services;

public class DeleteSupplierService(ISupplierRepository supplierRepository, ILogger<DeleteSupplierService> logger)
    : IDeleteSupplierService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(int supplierId, CancellationToken cancellationToken = default)
    {
        try
        {
            var supplier = await supplierRepository.GetByIdAsync(supplierId, cancellationToken);

            if (supplier is null)
            {
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Supplier with ID {supplierId} not found.");
            }

            var deleted = await supplierRepository.SoftDeleteAsync(supplierId, cancellationToken);

            if (deleted)
            {
                logger.LogInformation("Supplier {SupplierId} soft-deleted successfully", supplierId);
                return ServiceResult<bool>.Ok(true);
            }

            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to delete supplier.");
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error deleting supplier {SupplierId}", supplierId);
            string errMessage = $"Error deleting supplier, {e.Message}";
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
