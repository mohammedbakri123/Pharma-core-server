using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Customers.Dtos;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Customers.Services;

public class GetCustomerStatementService : IGetCustomerStatementService
{
    private readonly ICustomerRepository _customerRepository;

    public GetCustomerStatementService(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }

    public async Task<ServiceResult<CustomerStatementDto>> ExecuteAsync(int customerId, DateTime? from, DateTime? to, CancellationToken cancellationToken = default)
    {
        var customer = await _customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer == null)
            return ServiceResult<CustomerStatementDto>.Fail(ServiceErrorType.NotFound, "Customer not found.");

        var statement = await _customerRepository.GetStatementAsync(customerId, from, to, cancellationToken);

        return ServiceResult<CustomerStatementDto>.Ok(statement);
    }
}
