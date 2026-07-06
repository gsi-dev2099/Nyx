using System.Text;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Application.UseCases.Auth;
using CRM.ApiHub.Application.UseCases.Leads;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Infrastructure.Authentication;
using CRM.ApiHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace CRM.ApiHub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        // Configuración de Dapper para mapear snake_case (db) a PascalCase (C#)
        Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

        // DB
        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICampaignRepository, CampaignRepository>();
        services.AddScoped<ICatalogRepository, CatalogRepository>();
        services.AddScoped<IPreSaleRepository, PreSaleRepository>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<ILeadRepository, LeadRepository>();

        // Services & Stores
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IRefreshTokenStore, InMemoryRefreshTokenStore>();
        
        // Use Cases
        services.AddScoped<LoginUseCase>();
        services.AddScoped<MeUseCase>();
        services.AddScoped<RefreshTokenUseCase>();
        services.AddScoped<GetLeadsUseCase>();
        services.AddScoped<GetLeadByIdUseCase>();
        services.AddScoped<CreateLeadUseCase>();
        services.AddScoped<UpdateLeadStatusUseCase>();

        // JWT Authentication
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options => {
                options.TokenValidationParameters = new TokenValidationParameters 
                {
                    ValidateIssuer           = true,
                    ValidateAudience         = true,
                    ValidateLifetime         = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer   = config["JwtSettings:Issuer"],
                    ValidAudience = config["JwtSettings:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(config["JwtSettings:SecretKey"] ?? "a-default-secret-key-that-is-long-enough-for-validation-32-chars-long"))
                };
            });

        return services;
    }
}