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
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return ServiceResult<bool>.Fail(ServiceErrorType.ServerError, "Connection string not found");

            var backupPath = ResolveBackupPath(backupFile);
            if (backupPath is null)
                return ServiceResult<bool>.Fail(ServiceErrorType.Validation, "Backup file name is invalid");

            if (!File.Exists(backupPath))
                return ServiceResult<bool>.Fail(ServiceErrorType.NotFound, "Backup file not found");

            var (host, port, database, username, password) = ParseConnectionString(connectionString);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pg_restore",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            processStartInfo.ArgumentList.Add("-h");
            processStartInfo.ArgumentList.Add(host);
            processStartInfo.ArgumentList.Add("-p");
            processStartInfo.ArgumentList.Add(port);
            processStartInfo.ArgumentList.Add("-U");
            processStartInfo.ArgumentList.Add(username);
            processStartInfo.ArgumentList.Add("-d");
            processStartInfo.ArgumentList.Add(database);
            processStartInfo.ArgumentList.Add("--clean");
            processStartInfo.ArgumentList.Add("--create");
            processStartInfo.ArgumentList.Add(backupPath);
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

    private static string? ResolveBackupPath(string backupFile)
    {
        if (string.IsNullOrWhiteSpace(backupFile)
            || backupFile != Path.GetFileName(backupFile)
            || Path.GetExtension(backupFile) != ".sql")
        {
            return null;
        }

        var backupDir = Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "backups"));
        var backupPath = Path.GetFullPath(Path.Combine(backupDir, backupFile));

        return backupPath.StartsWith(backupDir + Path.DirectorySeparatorChar, StringComparison.Ordinal)
            ? backupPath
            : null;
    }
}
