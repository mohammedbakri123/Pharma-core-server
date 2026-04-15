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
        if (string.IsNullOrWhiteSpace(command.Name))
            return ServiceResult<MedicineDto>.Fail(ServiceErrorType.Validation, "Name is required.");

        var entity = PharmaCore.Domain.Entities.Medicine.Create(command.Name, command.ArabicName, command.Barcode, command.CategoryId, command.Unit);
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
            !created.IsDeleted.GetValueOrDefault(false),
            created.CreatedAt);

        return ServiceResult<MedicineDto>.Ok(dto);
    }
}
