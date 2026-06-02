using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;
using MedicineEntity = PharmaCore.Domain.Entities.Medicine;

namespace PharmaCore.Application.Medicine.Services;

public class ListMedicineService(IMedicineRepository medicineRepository, ILogger<ListMedicineService> logger)
    : IListMedicineService
{
    public async Task<ServiceResult<PagedResult<MedicineDto>>> ExecuteAsync(ListMedicineQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var limit = query.Limit <= 0 ? 20 : query.Limit;

            var result = await medicineRepository.ListAsync(
                page,
                limit,
                query.Search,
                query.Unit,
                query.CategoryId,
                cancellationToken);

            return ServiceResult<PagedResult<MedicineDto>>.Ok(MapToDto(result));
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error listing medicines");
            return ServiceResult<PagedResult<MedicineDto>>.Fail(ServiceErrorType.ServerError, $"Error listing medicines: {e.Message}");
        }
    }

    private static PagedResult<MedicineDto> MapToDto(PagedResult<MedicineEntity> result)
    {
        var items = result.Items
            .Select(m => new MedicineDto(
                m.MedicineId,
                m.Name,
                m.ArabicName,
                m.Barcode,
                m.CategoryId,
                null,
                m.Unit,
                !m.IsDeleted,
                m.CreatedAt))
            .ToList();

        return new PagedResult<MedicineDto>(items, result.Total, result.Page, result.Limit);
    }
}
