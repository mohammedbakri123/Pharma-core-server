using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Interfaces;

public interface IGetSaleByIdService
{
    Task<ServiceResult<SaleDetailsDto>> ExecuteAsync(GetSaleByIdQuery query, CancellationToken cancellationToken = default);
}