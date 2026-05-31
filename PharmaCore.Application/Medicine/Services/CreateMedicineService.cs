using Microsoft.Extensions.Logging;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.Medicine.Dtos;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Medicine.Services;

public class CreateMedicineService : ICreateMedicineService
{
    private readonly IMedicineRepository _repository;
    private readonly ILogger<CreateMedicineService> _logger;

    public CreateMedicineService(IMedicineRepository repository, ILogger<CreateMedicineService> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<ServiceResult<MedicineDto>> ExecuteAsync(CreateMedicineCommand command, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Name is required.");

            var entity = Domain.Entities.Medicine.Create(command.Name, command.ArabicName, command.Barcode,
                command.CategoryId, command.Unit);

            var nameExists = await _repository.ExistsByNameAsync(command.Name, cancellationToken: cancellationToken);
            
            if(nameExists)
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Name already exists.");

            var barcodeExists = await _repository.ExistsByBarcodeAsync(command.Barcode, cancellationToken: cancellationToken);
            
            if(barcodeExists)
                return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Barcode already exists.");

            var created = await _repository.AddAsync(entity, cancellationToken);

            _logger.LogInformation("Medicine '{Name}' created with ID {Id}", created.Name, created.MedicineId);

            var dto = new MedicineDto(
                created.MedicineId,
                created.Name,
                created.ArabicName,
                created.Barcode,
                created.CategoryId,
                null, // CategoryName will be populated when fetched with join
                created.Unit,
                !created.IsDeleted,
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
