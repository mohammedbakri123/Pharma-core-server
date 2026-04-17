using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class UpdateMedicineService : IUpdateMedicineService
{
    private readonly IMedicineRepository _medicineRepository;
    private readonly ILogger<UpdateMedicineService> _logger;

    public UpdateMedicineService(IMedicineRepository medicineRepository, ILogger<UpdateMedicineService> logger)
    {
        _medicineRepository = medicineRepository;
        _logger = logger;
    }

    public async Task<ServiceResult<MedicineDto>> ExecuteAsync(UpdateMedicineCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            var medicine = await _medicineRepository.GetByIdAsync(command.MedicineId, cancellationToken);

            if (medicine == null)
            {
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.NotFound, $"Medicine with ID {command.MedicineId} not found.");
            }

            if (command.Name != null)
            {
                var nameExists = await _medicineRepository.ExistsByNameAsync(command.Name, command.MedicineId, cancellationToken);
                if (nameExists)
                    return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Name already exists.");
                medicine.ChangeName(command.Name);
            }
            if (command.ArabicName != null)
                medicine.ChangeArabicName(command.ArabicName);
            if (command.Barcode != null)
            {
                var barcodeExists = await _medicineRepository.ExistsByBarcodeAsync(command.Barcode, command.MedicineId, cancellationToken);
                if (barcodeExists)
                    return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Barcode already exists.");
                medicine.SetBarcode(command.Barcode);
            }
            if (command.CategoryId.HasValue)
                medicine.AssignCategory(command.CategoryId);
            if (command.Unit.HasValue)
                medicine.SetUnit(command.Unit);

            var updated = await _medicineRepository.UpdateAsync(medicine, cancellationToken);

            _logger.LogInformation("Medicine '{Name}' updated successfully with ID {Id}", updated.Name, updated.MedicineId);

            return ServiceResult<MedicineDto>.Ok(MapToDto(updated));
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error updating medicine {MedicineId}", command.MedicineId);
            return ServiceResult<MedicineDto>.Fail(ServiceErrorType.ServerError, $"Error updating medicine: {e.Message}");
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
