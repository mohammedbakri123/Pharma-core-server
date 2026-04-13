using Microsoft.Extensions.DependencyInjection;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Application.Auth.Services;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Services;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Services;

namespace PharmaCore.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILoginService, LoginService>();
        services.AddScoped<IGetCurrentUserService, GetCurrentUserService>();
        services.AddScoped<IListUsersService, ListUsersService>();
        services.AddScoped<ICreateUserService, CreateUserService>();
        services.AddScoped<IUpdateUserService, UpdateUserService>();
        services.AddScoped<IDeleteUserService, DeleteUserService>();

        services.AddScoped<ICreateCategoryService, CreateCategoryService>();
        services.AddScoped<IUpdateCategoryService, UpdateCategoryService>();
        services.AddScoped<IDeleteCategoryService, DeleteCategoryService>();
        services.AddScoped<IListCategoriesService, ListCategoriesService>();
        services.AddScoped<IGetCategoryByIdService, GetCategoryByIdService>();

        return services;
    }
}
