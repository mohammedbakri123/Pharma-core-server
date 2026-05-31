using PharmaCore.Application.System.Dtos;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.System.Interfaces;

public interface IGetHealthCheckService
{
    Task<ServiceResult<HealthCheckDto>> ExecuteAsync(CancellationToken cancellationToken = default);
}

public interface IBackupDatabaseService
{
    Task<ServiceResult<BackupResultDto>> ExecuteAsync(string? backupName, CancellationToken cancellationToken = default);
}

public interface IRestoreDatabaseService
{
    Task<ServiceResult<bool>> ExecuteAsync(string backupFile, CancellationToken cancellationToken = default);
}
