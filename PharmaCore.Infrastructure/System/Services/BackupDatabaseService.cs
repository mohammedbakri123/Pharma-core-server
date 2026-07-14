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
            var connectionString = configuration.GetConnectionString("Default");
            if (string.IsNullOrEmpty(connectionString))
                return ServiceResult<BackupResultDto>.Fail(ServiceErrorType.ServerError, "Connection string not found");

            var timestamp = DateTime.UtcNow;
            var fileName = BuildBackupFileName(backupName, timestamp);
            if (fileName is null)
                return ServiceResult<BackupResultDto>.Fail(ServiceErrorType.Validation, "Backup name contains invalid characters");

            var backupDir = Path.Combine(Directory.GetCurrentDirectory(), "backups");
            
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            var backupPath = Path.Combine(backupDir, fileName);

            var (host, port, database, username, password) = ParseConnectionString(connectionString);

            var processStartInfo = new ProcessStartInfo
            {
                FileName = "pg_dump",
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
            processStartInfo.ArgumentList.Add("-F");
            processStartInfo.ArgumentList.Add("c");
            processStartInfo.ArgumentList.Add("-f");
            processStartInfo.ArgumentList.Add(backupPath);
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
                fileName,
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

    private static string? BuildBackupFileName(string? backupName, DateTime timestamp)
    {
        var name = string.IsNullOrWhiteSpace(backupName)
            ? $"backup_{timestamp:yyyyMMdd_HHmmss}"
            : Path.GetFileNameWithoutExtension(backupName.Trim());

        if (string.IsNullOrWhiteSpace(name)
            || name != Path.GetFileName(name)
            || name.Any(c => !(char.IsLetterOrDigit(c) || c is '_' or '-' or '.')))
        {
            return null;
        }

        return $"{name}.sql";
    }
}
