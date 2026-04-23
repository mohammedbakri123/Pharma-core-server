namespace PharmaCore.Application.System.Dtos;

public sealed record HealthCheckDto(
    string Status,
    DateTime Timestamp,
    string Database,
    string Uptime,
    string Version);
