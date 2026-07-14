using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using PharmaCore.API.Middleware;
using PharmaCore.Application;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Infrastructure;
using PharmaCore.Infrastructure.Security;
using Scalar.AspNetCore;

var envPath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envPath))
{
    foreach (var line in File.ReadAllLines(envPath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#')) continue;
        var parts = line.Split('=', 2);
        if (parts.Length != 2) continue;
        var key = parts[0].Trim();
        var val = parts[1].Trim().Trim('"').Trim('\'');
        Environment.SetEnvironmentVariable(key, val);
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder => policyBuilder
            .WithOrigins(
                "http://localhost:5000",
                "http://192.168.0.114:5000"
            )
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
if (IsUnsafeJwtSecret(jwtOptions.SecretKey))
{
    throw new InvalidOperationException(
        "Jwt:SecretKey must be configured with a non-placeholder secret of at least 32 characters.");
}

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey));

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtOptions.Issuer,
            ValidAudience = jwtOptions.Audience,
            IssuerSigningKey = signingKey,
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
                if (string.IsNullOrWhiteSpace(jti))
                {
                    context.Fail("Unauthorized");
                    return;
                }

                var tokenRevocationService = context.HttpContext.RequestServices.GetRequiredService<ITokenRevocationService>();
                if (await tokenRevocationService.IsRevokedAsync(jti, context.HttpContext.RequestAborted))
                {
                    context.Fail("Unauthorized");
                }
            }
        };
    });

builder.Services.AddAuthorization();
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy("AuthLogin", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();

// Add services to the container.
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "PharmaCore API",
        Version = "v1",
        Description = "API for the PharmaCore pharmaceutical management system.",
        Contact = new OpenApiContact { Name = "PharmaCore Team" }
    });

    // Include XML documentation comments from the API assembly
    var xmlFile = System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);

    // Include XML documentation comments from the Application assembly (for DTOs)
    var appXmlFile = "PharmaCore.Application.xml";
    var appXmlPath = Path.Combine(AppContext.BaseDirectory, appXmlFile);
    c.IncludeXmlComments(appXmlPath);

    // Read <response code="..."> XML tags and apply as response descriptions
    c.OperationFilter<PharmaCore.API.Filters.ResponseXmlTagFilter>(xmlPath);

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    c.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, securityScheme);
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            securityScheme,
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Seed database on startup
// using (var scope = app.Services.CreateScope())
// {
//     var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
//     await seeder.SeedAsync();
// }

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(c =>
    {
        c.RouteTemplate = "swagger/{documentName}/swagger.json";
    });
    app.UseSwaggerUI();
    app.MapScalarApiReference(options =>
    {
        options.OpenApiRoutePattern = "/swagger/v1/swagger.json";
    });
}
else
{
    app.UseHsts();
}

app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseHttpsRedirection();
app.Use(async (context, next) =>
{
    context.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    context.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    context.Response.Headers.TryAdd("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.TryAdd("Referrer-Policy", "strict-origin-when-cross-origin");
    await next();
});
app.UseRouting();
app.UseCors("AllowFrontend");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

static bool IsUnsafeJwtSecret(string? secret)
{
    if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
    {
        return true;
    }

    return secret.Contains("PLACEHOLDER", StringComparison.OrdinalIgnoreCase)
        || secret.Contains("YOUR_SUPER", StringComparison.OrdinalIgnoreCase)
        || secret.Contains("your-super", StringComparison.OrdinalIgnoreCase);
}
