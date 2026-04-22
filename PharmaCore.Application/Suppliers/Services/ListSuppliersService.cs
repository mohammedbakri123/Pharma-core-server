using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Suppliers.Dtos;
using PharmaCore.Application.Suppliers.Interfaces;
using PharmaCore.Application.Suppliers.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Suppliers.Services;

public class ListSuppliersService(ISupplierRepository supplierRepository, ILogger<ListSuppliersService> logger)
    : IListSuppliersService
{
    public async Task<ServiceResult<PagedResult<SupplierDto>>> ExecuteAsync(ListSuppliersQuery query,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var suppliers = await supplierRepository.ListAsync(cancellationToken);

            var filtered = suppliers.AsQueryable();

            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                var search = query.Search.ToLowerInvariant();
                filtered = filtered.Where(s =>
                    s.Name.ToLowerInvariant().Contains(search) ||
                    (s.PhoneNumber != null && s.PhoneNumber.ToLowerInvariant().Contains(search)));
            }

            var total = filtered.Count();
            var items = filtered
                .Skip((query.Page - 1) * query.Limit)
                .Take(query.Limit)
                .Select(s => new SupplierDto(s.SupplierId, s.Name, s.PhoneNumber, s.Address, s.CreatedAt))
                .ToList();

            return ServiceResult<PagedResult<SupplierDto>>.Ok(
                new PagedResult<SupplierDto>(items, total, query.Page, query.Limit));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting supplier list");
            string errMessage = $"Error getting supplier list, {e.Message}";
            return ServiceResult<PagedResult<SupplierDto>>.Fail(ServiceErrorType.ServerError, errMessage);
        }
    }
}
