using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Interfaces;

public interface IDeleteSupplierService
{
    Task<ServiceResult<bool>> ExecuteAsync(int supplierId, CancellationToken cancellationToken = default);
}
