using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class CreateMedicineService(IMedicineRepository repository, ILogger<CreateMedicineService> logger)
    : ICreateMedicineService
{
    public async Task<ServiceResult<MedicineDto>> ExecuteAsync(CreateMedicineCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Name is required.");

            var entity = Domain.Entities.Medicine.Create(command.Name, command.ArabicName, command.Barcode,
                command.CategoryId, command.Unit);

            var nameExists = await repository.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken);
            
            if(nameExists)
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Name already exists.");

            var barcodeExists = await repository.ExistsByBarcodeAsync(command.Barcode, cancellationToken: cancellationToken);
            
            if(barcodeExists)
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Barcode already exists.");

            var created = await repository.AddAsync(entity, cancellationToken);

            logger.LogInformation("Medicine '{Name}' created with ID {Id}", created.Name, created.MedicineId);

            var dto = new MedicineDto(
                created.MedicineId,
                created.Name,
                created.ArabicName,
                created.Barcode,
                created.CategoryId,
                null,
                created.Unit,
                created.CreatedAt);

            return ServiceResult<MedicineDto>.Ok(dto);
        }
        catch (Exception e)
        {
            string errorMessage = $"Error creating medicine: {e.Message} {e.StackTrace} {e.InnerException?.Message}";
            return ServiceResult<MedicineDto>.Fail(ServiceErrorType.ServerError, errorMessage);
        }
    }
}
