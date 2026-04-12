using PharmaCore.Application.Auth.Services;
using Microsoft.Extensions.DependencyInjection;
using PharmaCore.Application.Users.Services;

namespace PharmaCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<LoginService>();
        services.AddScoped<GetCurrentUserService>();
        services.AddScoped<ListUsersService>();
        services.AddScoped<CreateUserService>();
        services.AddScoped<UpdateUserService>();
        services.AddScoped<DeleteUserService>();

        return services;
    }
}
