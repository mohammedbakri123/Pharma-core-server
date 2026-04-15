using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerUnpaidSalesService : IGetCustomerUnpaidSalesService
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerUnpaidSalesService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ServiceResult<IReadOnlyList<UnpaidSaleDto>>> ExecuteAsync(int customerId, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
            return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Fail(ServiceErrorType.NotFound, "Customer not found.");

        var unpaidSales = await _customerRepository.GetUnpaidSalesAsync(customerId, cancellationToken);

        return ServiceResult<IReadOnlyList<UnpaidSaleDto>>.Ok(unpaidSales);
    }
}
