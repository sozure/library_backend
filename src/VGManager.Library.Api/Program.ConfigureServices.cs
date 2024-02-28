using CorrelationId.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using VGManager.Adapter.Client.Extensions;
using VGManager.Library.Api;
using VGManager.Library.Api.HealthChecks;
using VGManager.Library.Repositories.Boilerplate;
using VGManager.Library.Repositories.DbContexts;
using VGManager.Library.Repositories.Interfaces.SecretRepositories;
using VGManager.Library.Repositories.Interfaces.VGRepositories;
using VGManager.Library.Repositories.SecretRepositories;
using VGManager.Library.Repositories.VGRepositories;
using VGManager.Library.Services;
using VGManager.Library.Services.Interfaces;
using VGManager.Library.Services.Settings;
using ServiceProfiles = VGManager.Library.Services.MapperProfiles;

[ExcludeFromCodeCoverage]
static partial class Program
{
    static readonly string[] Tags = ["startup"];
    public static WebApplicationBuilder ConfigureServices(WebApplicationBuilder self, string specificOrigins)
    {
        var configuration = self.Configuration;
        var services = self.Services;

        services.AddDefaultCorrelationId(options =>
        {
            options.AddToLoggingScope = true;
        });

        services.AddCors(options =>
        {
            options.AddPolicy(name: specificOrigins,
                                policy =>
                                {
                                    policy.WithOrigins("http://localhost:3000")
                                    .AllowAnyMethod()
                                    .AllowAnyHeader();
                                });
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "VGManager.Library.Api", Version = "v1" });
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
            c.UseOneOfForPolymorphism();
        });

        services.AddAuthorization();
        services.AddControllers();
        services.AddHealthChecks()
            .AddCheck<StartupHealthCheck>(nameof(StartupHealthCheck), tags: Tags);

        services.AddAutoMapper(
            typeof(Program),
            typeof(ServiceProfiles.ChangesProfile)
        );

        services.AddOptions<OrganizationSettings>()
            .Bind(configuration.GetSection(Constants.SettingKeys.OrganizationSettings))
            .ValidateDataAnnotations();

        RegisterServices(services, configuration);

        return self;
    }

    private static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var databaseProviderKey = "DatabaseProvider";
        services.AddSingleton<StartupHealthCheck>();

        var dbConfig = new DatabaseConfiguration
        {
            ProviderKey = databaseProviderKey,
            PostgreConnectionStringKey = Constants.ConnectionStringKeys.PostgreSql,
            PostgreMigrationsAssemblyKey = Constants.MigrationAssemblyNames.PostgreSql,
        };

        services.AddDbContext<OperationsDbContext>(delegate (DbContextOptionsBuilder options)
        {
            options.UseNpgsql(
                configuration.GetConnectionString(dbConfig.PostgreConnectionStringKey),
                delegate (NpgsqlDbContextOptionsBuilder options)
                {
                    options.MigrationsAssembly(dbConfig.PostgreMigrationsAssemblyKey);
                }
            );
        }, ServiceLifetime.Scoped);

        services.SetupVGManagerAdapterClient(configuration);

        services.AddScoped<IVGAddColdRepository, VGAddColdRepository>();
        services.AddScoped<IVGDeleteColdRepository, VGDeleteColdRepository>();
        services.AddScoped<IVGUpdateColdRepository, VGUpdateColdRepository>();
        services.AddScoped<IKeyVaultCopyColdRepository, KeyVaultCopyColdRepository>();
        services.AddScoped<ISecretChangeColdRepository, SecretChangeColdRepository>();

        services.AddScoped<IVariableService, VariableService>();
        services.AddScoped<IVariableFilterService, VariableFilterService>();
        services.AddScoped<IVariableGroupService, VariableGroupService>();
        services.AddScoped<IKeyVaultService, KeyVaultService>();
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IChangeService, ChangeService>();
        services.AddScoped<IAdapterCommunicator, AdapterCommunicator>();
    }
}
