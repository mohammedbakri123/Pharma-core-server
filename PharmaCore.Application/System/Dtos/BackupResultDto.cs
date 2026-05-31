namespace PharmaCore.Application.System.Dtos;

public sealed record BackupResultDto(
    bool Success,
    string BackupFile,
    string BackupSize,
    DateTime Timestamp);
