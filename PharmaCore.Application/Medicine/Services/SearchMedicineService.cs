using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class SearchMedicineService : ISearchMedicineService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<SearchMedicineService> _logger;

    public SearchMedicineService(IMedicineRepository medicineRepository, ILogger<SearchMedicineService> logger)
    {
        _medicineRepository = medicineRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<PagedResult<MedicineDto>>> ExecuteAsync(SearchMedicineQuery query, CancellationToken cancellationToken = default)
    {
        try
        {
            var page = query.Page <= 0 ? 1 : query.Page;
            var limit = query.Limit <= 0 ? 20 : query.Limit;

            var medicines = await _medicineRepository.ListAsync(cancellationToken);
            
            // Apply search filter in memory
            var filtered = medicines.AsEnumerable();
            
            if (!string.IsNullOrWhiteSpace(query.Q))
            {
                var search = query.Q.ToLower();
                filtered = filtered.Where(m => 
                    m.Name.Contains(query.Q, StringComparison.OrdinalIgnoreCase) ||
                    (m.ArabicName != null && m.ArabicName.Contains(query.Q, StringComparison.OrdinalIgnoreCase)) ||
                    (m.Barcode != null && m.Barcode.Contains(query.Q, StringComparison.OrdinalIgnoreCase)));
            }
            
            var total = filtered.Count();
            var items = filtered
                .OrderByDescending(m => m.CreatedAt)
                .Skip((page - 1) * limit)
                .Take(limit)
                .Select(MapToDto)
                .ToList();

            return ServiceResult<PagedResult<MedicineDto>>.Ok(
                new PagedResult<MedicineDto>(items, total, page, limit));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error searching medicines with query '{Q}'", query.Q);
            return ServiceResult<PagedResult<MedicineDto>>.Fail(ServiceErrorType.ServerError, $"Error searching medicines: {e.Message}");
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
