using System.Collections.Concurrent;
using PharmaCore.Application.Abstractions.Auth;

namespace PharmaCore.Infrastructure.Security;

public class InMemoryTokenRevocationService : ITokenRevocationService
{
    private readonly ConcurrentDictionary<string, DateTime> _revokedTokens = new();

    public Task RevokeAsync(string tokenId, DateTime expiresAtUtc, CancellationToken cancellationToken = default)
    {
        _revokedTokens[tokenId] = expiresAtUtc;
        CleanupExpiredTokens();
        return Task.CompletedTask;
    }

    public Task<bool> IsRevokedAsync(string tokenId, CancellationToken cancellationToken = default)
    {
        CleanupExpiredTokens();
        return Task.FromResult(_revokedTokens.ContainsKey(tokenId));
    }

    private void CleanupExpiredTokens()
    {
        var now = DateTime.UtcNow;

        foreach (var revokedToken in _revokedTokens)
        {
            if (revokedToken.Value <= now)
            {
                _revokedTokens.TryRemove(revokedToken.Key, out _);
            }
        }
    }
}
