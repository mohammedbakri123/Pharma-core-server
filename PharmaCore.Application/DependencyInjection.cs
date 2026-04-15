using Microsoft.Extensions.DependencyInjection;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Application.Auth.Services;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Services;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Services;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Services;

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
        
        services.AddScoped<ICreateMedicineService, CreateMedicineService>();
        services.AddScoped<IUpdateMedicineService, UpdateMedicineService>();
        services.AddScoped<IDeleteMedicineService, DeleteMedicineService>();
        services.AddScoped<IListMedicineService, ListMedicineService>();
        services.AddScoped<IGetMedicineByIdService, GetMedicineByIdService>();
        services.AddScoped<ISearchMedicineService, SearchMedicineService>();

        return services;
    }
}
