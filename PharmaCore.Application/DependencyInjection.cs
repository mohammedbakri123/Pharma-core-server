using Microsoft.Extensions.DependencyInjection;
using PharmaCore.Application.Auth.Interfaces;
using PharmaCore.Application.Auth.Services;
using PharmaCore.Application.Categories.Interfaces;
using PharmaCore.Application.Categories.Services;
using PharmaCore.Application.Users.Interfaces;
using PharmaCore.Application.Users.Services;
using PharmaCore.Application.Medicine.Interfaces;
using PharmaCore.Application.Medicine.Services;
using PharmaCore.Application.Customers.Interfaces;
using PharmaCore.Application.Customers.Services;
using PharmaCore.Application.Payments.Interfaces;
using PharmaCore.Application.Payments.Services;

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

        services.AddScoped<ICreateCustomerService, CreateCustomerService>();
        services.AddScoped<IUpdateCustomerService, UpdateCustomerService>();
        services.AddScoped<IDeleteCustomerService, DeleteCustomerService>();
        services.AddScoped<IListCustomersService, ListCustomersService>();
        services.AddScoped<IGetCustomerByIdService, GetCustomerByIdService>();
        services.AddScoped<IGetCustomerSalesService, GetCustomerSalesService>();
        services.AddScoped<IGetCustomerDebtService, GetCustomerDebtService>();
        services.AddScoped<IGetCustomerUnpaidSalesService, GetCustomerUnpaidSalesService>();
        services.AddScoped<IGetCustomerStatementService, GetCustomerStatementService>();
        services.AddScoped<IPayCustomerDebtService, PayCustomerDebtService>();
        services.AddScoped<ICreatePaymentService, CreatePaymentService>();
        services.AddScoped<IListPaymentsService, ListPaymentsService>();
        services.AddScoped<IGetPaymentByIdService, GetPaymentByIdService>();
        services.AddScoped<IGetPaymentsByReferenceService, GetPaymentsByReferenceService>();

        return services;
    }
}
