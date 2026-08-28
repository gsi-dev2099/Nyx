using System.Text;
using CRM.ApiHub.Application.Interfaces;
using CRM.ApiHub.Application.UseCases.Auth;
using CRM.ApiHub.Application.UseCases.Leads;
using CRM.ApiHub.Application.UseCases.SalesOrders;
using CRM.ApiHub.Application.UseCases.Documents;
using CRM.ApiHub.Application.UseCases.Supervisor;
using CRM.ApiHub.Application.UseCases.Backoffice;
using CRM.ApiHub.Application.UseCases.Audit;
using CRM.ApiHub.Application.UseCases.KB;
using CRM.ApiHub.Application.UseCases.Commissions;
using CRM.ApiHub.Application.UseCases.Providers;
using CRM.ApiHub.Application.UseCases.Activations;
using CRM.ApiHub.Application.UseCases.Reports;
using CRM.ApiHub.Domain.Repositories;
using CRM.ApiHub.Infrastructure.Authentication;
using CRM.ApiHub.Infrastructure.Persistence;
using CRM.ApiHub.Infrastructure.Services;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

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
        services.AddScoped<ISalesOrderRepository, SalesOrderRepository>();
        services.AddScoped<IOrderDocumentRepository, OrderDocumentRepository>();
        services.AddScoped<ISupervisorRepository, SupervisorRepository>();
        services.AddScoped<IBackofficeRepository, BackofficeRepository>();
        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IIncidentRepository, IncidentRepository>();
        services.AddScoped<IOrderDataRepository, OrderDataRepository>();
        services.AddScoped<IFormRepository, FormRepository>();
        services.AddScoped<IApprovalRepository, ApprovalRepository>();
        services.AddScoped<IAlternateProfileRepository, AlternateProfileRepository>();
        services.AddScoped<IAuditRepository, AuditRepository>();
        services.AddScoped<IKnowledgeBaseRepository, KnowledgeBaseRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<ICommissionRepository, CommissionRepository>();
        services.AddScoped<IProviderRepository, ProviderRepository>();
        services.AddScoped<IActivationRepository, ActivationRepository>();
        services.AddScoped<IReportRepository, ReportRepository>();
        services.AddScoped<IUserPreferencesRepository, UserPreferencesRepository>();

        // Services & Stores
        services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IConnectionMultiplexer>(sp => {
            var configuration = sp.GetRequiredService<IConfiguration>();
            var connStr = configuration["RedisSettings:ConnectionString"];
            if (string.IsNullOrEmpty(connStr)) return null!;
            try {
                return ConnectionMultiplexer.Connect(connStr);
            } catch {
                return null!; // Fallback to InMemory
            }
        });
        services.AddSingleton<IRefreshTokenStore, RedisRefreshTokenStore>();
        
        services.AddScoped<INotificationService, Application.Services.NotificationService>();

        // SLA Engine Client (Typed HttpClient)
        services.AddHttpClient<ISlaEngineClient, SlaEngineClient>(client =>
        {
            var baseUrl = config["SlaEngineSettings:BaseUrl"] ?? "http://sla_engine_api:5070";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        }).AddStandardResilienceHandler();

        // Flow Engine Client (Typed HttpClient)
        services.AddHttpClient<IFlowEngineClient, FlowEngineClient>(client =>
        {
            var baseUrl = config["FlowEngineSettings:BaseUrl"] ?? "http://flow_engine_api:5072";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        }).AddStandardResilienceHandler();

        // Approval Engine Client (Typed HttpClient)
        services.AddHttpClient<IApprovalEngineClient, ApprovalEngineClient>(client =>
        {
            var baseUrl = config["ApprovalEngineSettings:BaseUrl"] ?? "http://approval_engine_api:5071";
            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5);
        }).AddStandardResilienceHandler();

        // SignalR & Custom UserId & Redis Backplane
        var signalRBuilder = services.AddSignalR();
        var redisConnStr = config["RedisSettings:ConnectionString"];
        if (!string.IsNullOrEmpty(redisConnStr))
        {
            try
            {
                using var redisTest = ConnectionMultiplexer.Connect(redisConnStr);
                if (redisTest.IsConnected)
                {
                    signalRBuilder.AddStackExchangeRedis(redisConnStr, options =>
                    {
                        options.Configuration.ChannelPrefix = RedisChannel.Literal("NyxCRM");
                    });
                }
            }
            catch
            {
                // Fallback a SignalR en memoria si Redis no está disponible localmente
            }
        }
        services.AddSingleton<Microsoft.AspNetCore.SignalR.IUserIdProvider, CRM.ApiHub.Infrastructure.Authentication.CustomUserIdProvider>();
        // Use Cases
        services.AddScoped<LoginUseCase>();
        services.AddScoped<MeUseCase>();
        services.AddScoped<RefreshTokenUseCase>();
        services.AddScoped<GetLeadsUseCase>();
        services.AddScoped<GetLeadByIdUseCase>();
        services.AddScoped<CreateLeadUseCase>();
        services.AddScoped<UpdateLeadStatusUseCase>();
        
        // Sales Orders Use Cases
        services.AddScoped<GetSalesOrdersUseCase>();
        services.AddScoped<GetSalesOrderByIdUseCase>();
        services.AddScoped<CreateSalesOrderUseCase>();
        services.AddScoped<UpdateSalesOrderStatusUseCase>();
        services.AddScoped<GetSalesOrderHistoryUseCase>();

        // Document Use Cases
        services.AddScoped<GetDocumentsByOrderUseCase>();
        services.AddScoped<GetDocumentByIdUseCase>();
        services.AddScoped<UploadOrderDocumentUseCase>();
        services.AddScoped<VerifyOrderDocumentUseCase>();

        // Supervisor Use Cases
        services.AddScoped<GetTeamOrdersUseCase>();
        services.AddScoped<GetTeamStatsUseCase>();
        services.AddScoped<BulkTransferToBackofficeUseCase>();

        // Backoffice Use Cases
        services.AddScoped<GetAssignedOrdersUseCase>();
        services.AddScoped<GetPendingVerificationUseCase>();
        services.AddScoped<UpdateBackofficeOrderStatusUseCase>();
        services.AddScoped<VerifyBackofficeDocumentUseCase>();

        // Audit Use Cases
        services.AddScoped<GetChecklistUseCase>();
        services.AddScoped<CreateAuditUseCase>();
        services.AddScoped<SaveAuditItemUseCase>();
        services.AddScoped<CloseAuditUseCase>();

        // KB Use Cases
        services.AddScoped<SearchKbArticlesUseCase>();
        services.AddScoped<GetKbArticleByIdUseCase>();
        services.AddScoped<SubmitKbFeedbackUseCase>();

        // Currency & Commission Use Cases
        services.AddScoped<GetCurrenciesUseCase>();
        services.AddScoped<ConvertAmountUseCase>();
        services.AddScoped<GetSettlementsUseCase>();
        services.AddScoped<CreateSettlementUseCase>();
        services.AddScoped<AddSettlementItemsUseCase>();
        services.AddScoped<UpdateSettlementStatusUseCase>();
        services.AddScoped<DeleteSettlementUseCase>();

        // Provider Use Cases
        services.AddScoped<GetProviderCatalogUseCase>();
        services.AddScoped<GetProviderStatusMappingUseCase>();
        services.AddScoped<LogProviderSyncUseCase>();
        services.AddScoped<UpdateOrderProviderStatusUseCase>();

        // Activation Use Cases
        services.AddScoped<GetPendingActivationsUseCase>();
        services.AddScoped<GetActivationsByOrderUseCase>();
        services.AddScoped<UpdateActivationUseCase>();
        services.AddScoped<GetDelayedActivationsUseCase>();

        // Report Use Cases
        services.AddScoped<GetConversionFunnelUseCase>();
        services.AddScoped<GetSalesByAsesorUseCase>();
        services.AddScoped<GetIncidentStatsUseCase>();
        services.AddScoped<GetActivationStatsUseCase>();

        // JWT Authentication
        var secretKey = config["JwtSettings:SecretKey"];
        if (string.IsNullOrEmpty(secretKey))
        {
            throw new InvalidOperationException("La clave de firma JWT 'JwtSettings:SecretKey' no está configurada en la aplicación.");
        }

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
                        Encoding.UTF8.GetBytes(secretKey))
                };
            });

        return services;
    }
}