using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Application.Abstractions.Persistence;
using PharmaCore.Application.System.Interfaces;
using PharmaCore.Infrastructure.Persistence;
using PharmaCore.Infrastructure.Persistence.Repositories;
using PharmaCore.Infrastructure.Security;
using PharmaCore.Infrastructure.System.Services;

namespace PharmaCore.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("Default")));

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.AddSingleton(resolver => resolver.GetRequiredService<IOptions<JwtOptions>>().Value);
        services.AddSingleton<ITokenRevocationService, InMemoryTokenRevocationService>();
        services.AddScoped<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IMedicineRepository, MedicineRepository>();
        services.AddScoped<IBatchRepository, BatchRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISaleRepository, SaleRepository>();
        services.AddScoped<IPaymentRepository, PaymentRepository>();
        services.AddScoped<ISalesReturnRepository, SalesReturnRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IAdjustmentRepository, AdjustmentRepository>();
        services.AddScoped<IExpenseRepository, ExpenseRepository>();
        services.AddScoped<ISupplierRepository, SupplierRepository>();
        services.AddScoped<IPurchaseRepository, PurchaseRepository>();
        services.AddScoped<IPurchaseReturnRepository, PurchaseReturnRepository>();

        services.AddScoped<IGetHealthCheckService, GetHealthCheckService>();

        return services;
    }
}
