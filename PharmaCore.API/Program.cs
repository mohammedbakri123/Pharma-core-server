using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using Microsoft.IdentityModel.Tokens;
using PharmaCore.Application;
using PharmaCore.Application.Abstractions.Auth;
using PharmaCore.Infrastructure;
using PharmaCore.Infrastructure.Security;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policyBuilder => policyBuilder
            .WithOrigins("http://localhost:5000")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials());
});

var jwtOptions = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
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

// Add services to the container.
builder.Services.AddControllers();
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

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
