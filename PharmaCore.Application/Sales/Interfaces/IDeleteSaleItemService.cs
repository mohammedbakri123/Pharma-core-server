using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface IDeleteSaleItemService
{
    Task<ServiceResult<bool>> ExecuteAsync(DeleteSaleItemCommand command, CancellationToken cancellationToken = default);
}