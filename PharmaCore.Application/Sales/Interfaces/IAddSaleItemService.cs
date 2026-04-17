using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface IAddSaleItemService
{
    Task<ServiceResult<SaleItemDto>> ExecuteAsync(AddSaleItemCommand command, CancellationToken cancellationToken = default);
}