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
using PharmaCore.Application.Sales.Interfaces;
using PharmaCore.Application.Sales.Services;
using PharmaCore.Application.SalesReturn.Interfaces;
using PharmaCore.Application.SalesReturn.Services;
using PharmaCore.Application.Inventory.Interfaces;
using PharmaCore.Application.Inventory.Services;
using PharmaCore.Application.Suppliers.Interfaces;
using PharmaCore.Application.Suppliers.Services;
using PharmaCore.Application.Expenses.Interfaces;
using PharmaCore.Application.Expenses.Services;

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
        services.AddScoped<IListDeletedCategoriesService, ListDeletedCategoriesService>();
        services.AddScoped<IRestoreCategoryService, RestoreCategoryService>();
        services.AddScoped<IGetCategoryByIdService, GetCategoryByIdService>();
        services.AddScoped<IHardDeleteCategoryService, HardDeleteCategoryService>();

        
        services.AddScoped<ICreateMedicineService, CreateMedicineService>();
        services.AddScoped<IUpdateMedicineService, UpdateMedicineService>();
        services.AddScoped<IDeleteMedicineService, DeleteMedicineService>();
        services.AddScoped<IListMedicineService, ListMedicineService>();
        services.AddScoped<IListDeletedMedicinesService, ListDeletedMedicinesService>();
        services.AddScoped<IRestoreMedicineService, RestoreMedicineService>();
        services.AddScoped<IGetMedicineByIdService, GetMedicineByIdService>();
        services.AddScoped<ISearchMedicineService, SearchMedicineService>();
        services.AddScoped<IHardDeleteMedicineService, HardDeleteMedicineService>();

        services.AddScoped<ICreateCustomerService, CreateCustomerService>();
        services.AddScoped<IUpdateCustomerService, UpdateCustomerService>();
        services.AddScoped<IDeleteCustomerService, DeleteCustomerService>();
        services.AddScoped<IListCustomersService, ListCustomersService>();
        services.AddScoped<IListDeletedCustomersService, ListDeletedCustomersService>();
        services.AddScoped<IRestoreCustomerService, RestoreCustomerService>();
        services.AddScoped<IGetCustomerByIdService, GetCustomerByIdService>();
        services.AddScoped<IPayCustomerDebtService, PayCustomerDebtService>();
        
        services.AddScoped<ICreatePaymentService, CreatePaymentService>();
        services.AddScoped<IListPaymentsService, ListPaymentsService>();
        services.AddScoped<IGetPaymentByIdService, GetPaymentByIdService>();
        services.AddScoped<IGetPaymentsByReferenceService, GetPaymentsByReferenceService>();
        
        services.AddScoped<ICreateSaleService, CreateSaleService>();
        services.AddScoped<IListSalesService, ListSalesService>();
        services.AddScoped<IGetSaleByIdService, GetSaleByIdService>();
        services.AddScoped<IAddSaleItemService, AddSaleItemService>();
        services.AddScoped<IUpdateSaleItemService, UpdateSaleItemService>();
        services.AddScoped<IDeleteSaleItemService, DeleteSaleItemService>();
        services.AddScoped<ICompleteSaleService, CompleteSaleService>();
        services.AddScoped<ICancelSaleService, CancelSaleService>();
        services.AddScoped<IGetSaleBalanceService, GetSaleBalanceService>();
        services.AddScoped<IGetUnpaidSalesService, GetUnpaidSalesService>();
        services.AddScoped<IGetSalesSummaryService, GetSalesSummaryService>();
        services.AddScoped<IGetSalesStatementService, GetSalesStatementService>();

        services.AddScoped<ICreateSalesReturnService, CreateSalesReturnService>();
        services.AddScoped<IAddSalesReturnItemService, AddSalesReturnItemService>();
        services.AddScoped<IListSalesReturnService, ListSalesReturnService>();
        services.AddScoped<IGetSalesReturnByIdService, GetSalesReturnByIdService>();
        services.AddScoped<IUpdateSalesReturnService, UpdateSalesReturnService>();
        services.AddScoped<IDeleteSalesReturnService, DeleteSalesReturnService>();
        services.AddScoped<IUpdateSalesReturnItemService, UpdateSalesReturnItemService>();
        services.AddScoped<IDeleteSalesReturnItemService, DeleteSalesReturnItemService>();

        services.AddScoped<IGetStockService, GetStockService>();
        services.AddScoped<IGetStockByMedicineService, GetStockByMedicineService>();
        services.AddScoped<IGetLowStockService, GetLowStockService>();
        services.AddScoped<IGetExpiringService, GetExpiringService>();
        services.AddScoped<ICreateAdjustmentService, CreateAdjustmentService>();

        services.AddScoped<ICreateSupplierService, CreateSupplierService>();
        services.AddScoped<IUpdateSupplierService, UpdateSupplierService>();
        services.AddScoped<IDeleteSupplierService, DeleteSupplierService>();
        services.AddScoped<IListSuppliersService, ListSuppliersService>();
        services.AddScoped<IGetSupplierByIdService, GetSupplierByIdService>();

        services.AddScoped<ICreateExpenseService, CreateExpenseService>();
        services.AddScoped<IListExpensesService, ListExpensesService>();
        services.AddScoped<IDeleteExpenseService, DeleteExpenseService>();

        return services;
    }
}
