namespace PharmaCore.Application.Users.Interfaces;

public interface IDeleteUserService
{
    Task ExecuteAsync(int userId, CancellationToken cancellationToken = default);
}
