using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Sales.Dtos;
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Sales.Services;

public class GetUnpaidSalesService : IGetUnpaidSalesService
{
    private readonly ISaleRepository _saleRepository;

    public GetUnpaidSalesService(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<UnpaidSaleDto>>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var unpaidSales = await _saleRepository.GetUnpaidSalesByCustomerIdAsync(customerId, cancellationToken);
        return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Ok(unpaidSales);
    }
}
