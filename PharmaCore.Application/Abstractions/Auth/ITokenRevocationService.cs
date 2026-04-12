namespace PharmaCore.Application.Abstractions.Auth;

public interface ITokenRevocationService
{
    Task RevokeAsync(string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken = default);
    Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default);
}
