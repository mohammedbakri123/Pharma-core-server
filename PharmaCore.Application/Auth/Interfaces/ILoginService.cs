using PharmaCore.Application.Auth.Dtos;
using PharmaCore.Application.Auth.Requests;
using PharmaCore.Domain.Shared;

namespace PharmaCore.Application.Auth.Interfaces;

public interface ILoginService
{
    Task<ServiceResult<LoginResponseDto>> ExecuteAsync(LoginCommand command, CancellationToken cancellationToken = default);
}
