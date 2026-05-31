using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface IUpdateSaleItemService
{
    Task<ServiceResult<SaleItemDto>> ExecuteAsync(UpdateSaleItemCommand command, CancellationToken cancellationToken = default);
}