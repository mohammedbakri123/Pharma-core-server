using PharmaCore.Application.Common.Pagination;
using MedicineEntity = PharmaCore.Domain.Entities.Medicine;
using PharmaCore.Domain.Enums;

namespace PharmaCore.Application.Abstractions.Persistence;
public interface IMedicineRepository
{
    Task<MedicineEntity?> GetByIdAsync(int medicineId, CancellationToken cancellationToken = default);
    Task<IEnumerable<MedicineEntity>> ListAsync(CancellationToken cancellationToken = default);
    Task<MedicineEntity> AddAsync(MedicineEntity medicine, CancellationToken cancellationToken = default);
    Task<MedicineEntity> UpdateAsync(MedicineEntity medicine, CancellationToken cancellationToken = default);
    Task<bool> SoftDeleteAsync(int medicineId, CancellationToken cancellationToken = default);
    Task<bool> HardDeleteAsync(int medicineId, CancellationToken cancellationToken = default);

    Task<int> CountAsync(string? searchTerm, MedicineUnit? unit, int? categoryId, CancellationToken cancellationToken = default);
    Task<bool> ExistsByNameAsync(string? name, int? excludeMedicineId = null, CancellationToken cancellationToken = default);
    Task<bool> ExistsByBarcodeAsync(string? barcode, int? excludeMedicineId = null, CancellationToken cancellationToken = default);
    
}
