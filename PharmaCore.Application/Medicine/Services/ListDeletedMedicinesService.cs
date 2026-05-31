using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class ListDeletedMedicinesService : IListDeletedMedicinesService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<ListDeletedMedicinesService> _logger;

    public ListDeletedMedicinesService(IMedicineRepository medicineRepository, ILogger<ListDeletedMedicinesService> logger)
    {
        _medicineRepository = medicineRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<MedicineDto>>> ExecuteAsync(ListDeletedMedicinesQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var limit = query.Limit <= 0 ? 20 : query.Limit;

            var medicines = await _medicineRepository.ListDeletedAsync(cancellationToken);
            
            // Apply filters in memory
            var filtered = medicines.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(query.Search))
            {
                filtered = filtered.Where(m => 
                    m.Name.Contains(query.Search, StringComparison.OrdinalIgnoreCase) ||
                    (m.ArabicName != null && m.ArabicName.Contains(query.Search, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Barcode != null && m.Barcode.Contains(query.Search, StringComparison.OrdinalIgnoreCase)));
            }
            
            if (query.Unit.HasValue)
            {
                filtered = filtered.Where(m => m.Unit == query.Unit.Value);
            }
            
            if (query.CategoryId.HasValue)
            {
                filtered = filtered.Where(m => m.CategoryId == query.CategoryId.Value);
            }
            
            var total = filtered.Count();
            var items = filtered
                .OrderByDescending(m => m.DeletedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(MapToDto)
                .ToList();

            return ServiceResult<PagedResult<MedicineDto>>.Ok(
                new PagedResult<MedicineDto>(items, total, page, limit));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error listing deleted medicines");
            return ServiceResult<PagedResult<MedicineDto>>.Fail(ServiceErrorType.ServerError, $"Error listing deleted medicines: {e.Message}");
        }
    }

    private static MedicineDto MapToDto(PharmaCore.Domain.Entities.Medicine m) =>
        new MedicineDto(
            m.MedicineId,
            m.Name,
            m.ArabicName,
            m.Barcode,
            m.CategoryId,
            null,
            m.Unit,
            !m.IsDeleted,
            m.CreatedAt);
}
