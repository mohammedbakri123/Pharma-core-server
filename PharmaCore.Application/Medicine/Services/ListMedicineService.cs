using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Common.Pagination;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class ListMedicineService : IListMedicineService
{
    private readonly IMedicineRepository _medicineRepository;

    public ListMedicineService(IMedicineRepository medicineRepository)
    {
        _medicineRepository = medicineRepository;
    }

    public async Task<ServiceResult<PagedResult<MedicineDto>>> ExecuteAsync(ListMedicineQuery query, CancellationToken cancellationToken = default)
    {
        var page = query.Page <= 0 ? 1 : query.Page;
        var limit = query.Limit <= 0 ? 20 : query.Limit;

        var medicines = await _medicineRepository.GetPagedAsync(page, limit, query.Search, query.Unit, query.CategoryId, cancellationToken);
        var total = await _medicineRepository.CountAsync(query.Search, query.Unit, query.CategoryId, cancellationToken);

        var items = medicines.Select(MapToDto).ToList();

        return ServiceResult<PagedResult<MedicineDto>>.Ok(
            new PagedResult<MedicineDto>(items, total, page, limit));
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
            !m.IsDeleted.GetValueOrDefault(false),
            m.CreatedAt);
}
