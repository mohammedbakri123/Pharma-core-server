using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;
using MedicineEntity = PharmaCore.Domain.Entities.Medicine;

namespace PharmaCore.Application.Medicine.Services;

public class ListDeletedMedicinesService(
    IMedicineRepository medicineRepository,
    ILogger<ListDeletedMedicinesService> logger)
    : IListDeletedMedicinesService
{
    public async Task<ServiceResult<PagedResult<MedicineDto>>> ExecuteAsync(ListDeletedMedicinesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var limit = query.Limit <= 0 ? 20 : query.Limit;

            var result = await medicineRepository.ListDeletedAsync(
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
            logger.LogError(e, "Error listing deleted medicines");
            return ServiceResult<PagedResult<MedicineDto>>.Fail(ServiceErrorType.ServerError, $"Error listing deleted medicines: {e.Message}");
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
               
                m.Unit,
                !m.IsDeleted,
                m.CreatedAt))
            .ToList();

        return new PagedResult<MedicineDto>(items, result.Total, result.Page, result.Limit);
    }
}
