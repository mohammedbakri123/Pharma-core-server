using PharmaCore.Domain.Entities;

namespace PharmaCore.Application.Abstractions.Auth;

public interface ITokenService
{
    string CreateToken(User user);
}
