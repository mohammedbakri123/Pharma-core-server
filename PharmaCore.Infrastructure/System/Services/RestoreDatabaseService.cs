using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PharmaCore.Application.System.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Infrastructure.System.Services;

public class RestoreDatabaseService(
    IConfiguration configuration,
    ILogger<RestoreDatabaseService> logger)
    : IRestoreDatabaseService
{
    public async Task<ServiceResult<bool>> ExecuteAsync(string backupFile, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Connection string not found");

            if (!File.Exists(backupFile))
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, $"Backup file not found: {backupFile}");

            var (host, port, database, username, password) = ParseConnectionString(connectionString);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pg_restore",
                Arguments = $"-h {host} -p {port} -U {username} -d {database} --clean --create \"{backupFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            processStartInfo.EnvironmentVariables["PGPASSWORD"] = password;

            using var process = Process.Start(processStartInfo);
            if (process == null)
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Failed to start pg_restore process");

            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                logger.LogError("pg_restore failed: {Error}", error);
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, $"Restore failed: {error}");
            }

            return ServiceResult<bool>.Ok(true);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during database restore");
            return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, e.Message);
        }
    }

    private static (string host, string port, string database, string username, string password) ParseConnectionString(string connectionString)
    {
        var host = "localhost";
        var port = "5432";
        var database = "";
        var username = "";
        var password = "";

        var parts = connectionString.Split(';');
        foreach (var part in parts)
        {
            var kv = part.Split('=');
            if (kv.Length != 2) continue;

            switch (kv[0].Trim().ToLower())
            {
                case "host": host = kv[1]; break;
                case "port": port = kv[1]; break;
                case "database": database = kv[1]; break;
                case "username": username = kv[1]; break;
                case "password": password = kv[1]; break;
            }
        }

        return (host, port, database, username, password);
    }
}
