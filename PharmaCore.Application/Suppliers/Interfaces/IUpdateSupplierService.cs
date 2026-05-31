using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Interfaces;

public interface IUpdateSupplierService
{
    Task<ServiceResult<SupplierDto>> ExecuteAsync(UpdateSupplierCommand command, CancellationToken cancellationToken = default);
}
