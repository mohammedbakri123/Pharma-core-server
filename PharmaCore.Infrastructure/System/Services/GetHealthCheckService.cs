using Microsoft.EntityFrameworkCore;
using PharmaCore.Application.System.Dtos;
using PharmaCore.Application.System.Interfaces;
using PharmaCore.Domain.Shared;
using PharmaCore.Infrastructure.Persistence;

namespace PharmaCore.Infrastructure.System.Services;

public class GetHealthCheckService(
    ApplicationDbContext dbContext)
    : IGetHealthCheckService
{
    public async Task<ServiceResult<HealthCheckDto>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var databaseStatus = "disconnected";
            
            try
            {
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);
                databaseStatus = canConnect ? "connected" : "disconnected";
            }
            catch
            {
                databaseStatus = "disconnected";
            }

            var uptime = DateTime.UtcNow - startTime;
            var version = typeof(GetHealthCheckService).Assembly.GetName().Version?.ToString() ?? "1.0.0";

            var status = databaseStatus == "connected" ? "ok" : "degraded";

            var dto = new HealthCheckDto(
                status,
                DateTime.UtcNow,
                databaseStatus,
                uptime.ToString(@"hh\:mm\:ss"),
                version);

            return ServiceResult<HealthCheckDto>.Ok(dto);
        }
        catch (Exception e)
        {
            return ServiceResult<HealthCheckDto>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }
}
