using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface ICreateSaleService
{
    Task<ServiceResult<SaleDto>> ExecuteAsync(CreateSaleCommand command, CancellationToken cancellationToken = default);
}