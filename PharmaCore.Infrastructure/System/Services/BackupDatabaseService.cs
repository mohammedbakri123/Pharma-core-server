using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PharmaCore.Application.System.Dtos;
using PharmaCore.Application.System.Interfaces;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Infrastructure.System.Services;

public class BackupDatabaseService(
    IConfiguration configuration,
    ILogger<BackupDatabaseService> logger)
    : IBackupDatabaseService
{
    public async Task<ServiceResult<BackupResultDto>> ExecuteAsync(string? backupName, CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
                return ServiceResult<BackupResultDto>.Fail(ServiceErrorType.ServerError, "Connection string not found");

            var timestamp = DateTime.UtcNow;
            var fileName = backupName ?? $"backup_{timestamp:yyyyMMdd_HHmmss}";
            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            var backupPath = Path.Combine(backupDir, $"{fileName}.sql");

            var (host, port, database, username, password) = ParseConnectionString(connectionString);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
                Arguments = $"-h {host} -p {port} -U {username} -d {database} -F c -f \"{backupPath}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            processStartInfo.EnvironmentVariables["PGPASSWORD"] = password;

            using var process = Process.Start(processStartInfo);
            if (process == null)
                return ServiceResult<BackupResultDto>.Fail(ServiceErrorType.ServerError, "Failed to start pg_dump process");

            var error = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                logger.LogError("pg_dump failed: {Error}", error);
                return ServiceResult<BackupResultDto>.Fail(ServiceErrorType.ServerError, $"Backup failed: {error}");
            }

            var fileInfo = new FileInfo(backupPath);
            var size = FormatFileSize(fileInfo.Length);

            var result = new BackupResultDto(
                true,
                backupPath,
                size,
                timestamp);

            return ServiceResult<BackupResultDto>.Ok(result);
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error during database backup");
            return ServiceResult<BackupResultDto>.Fail(ServiceErrorType.ServerError, e.Message);
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

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        var order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size = size / 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
