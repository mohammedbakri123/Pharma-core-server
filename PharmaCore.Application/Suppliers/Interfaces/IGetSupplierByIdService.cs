using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Interfaces;

public interface IGetSupplierByIdService
{
    Task<ServiceResult<SupplierDto>> ExecuteAsync(GetSupplierByIdQuery query, CancellationToken cancellationToken = default);
}
