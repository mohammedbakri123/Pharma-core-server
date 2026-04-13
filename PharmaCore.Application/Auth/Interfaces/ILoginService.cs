using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Requests;

namespace PharmaCore.Application.Auth.Interfaces;

public interface ILoginService
{
    Task<LoginResponseDto> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default);
}
