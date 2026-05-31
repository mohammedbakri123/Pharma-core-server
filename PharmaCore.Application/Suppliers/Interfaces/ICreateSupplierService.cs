using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Interfaces;

public interface ICreateSupplierService
{
    Task<ServiceResult<SupplierDto>> ExecuteAsync(CreateSupplierCommand command, CancellationToken cancellationToken = default);
}
