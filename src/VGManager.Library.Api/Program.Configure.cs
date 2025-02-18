using CorrelationId;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using VGManager.Communication.Kafka.Extensions;
using VGManager.Library.Api.Endpoints.Changes;
using VGManager.Library.Api.Endpoints.Project;
using VGManager.Library.Api.Endpoints.Secret;
using VGManager.Library.Api.Endpoints.VariableGroup;
using VGManager.Library.Api.HealthChecks;
using VGManager.Library.Repositories.DbContexts;

namespace VGManager.Library.Api;

static partial class Program
{
    internal static async Task<WebApplication> ConfigureAsync(WebApplication app, string specificOrigins)
    {
        var configuration = app.Configuration;

        var healthCheckSettings = configuration
                .GetSection(Constants.SettingKeys.HealthChecksSettings)
                .Get<HealthChecksSettings>()
                ?? throw new InvalidOperationException("HealthChecksSettings is missing from configuration.");

        app.UseHealthChecks("/health/startup", healthCheckSettings.Port, new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("startup")
        });

        app.UseHealthChecks("/health/liveness", healthCheckSettings.Port, new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("liveness")
        });

        app.UseHealthChecks("/health/readiness", healthCheckSettings.Port, new HealthCheckOptions
        {
            Predicate = healthCheck => healthCheck.Tags.Contains("readiness")
        });

        app.UseCorrelationIdValidation();
        app.UseCorrelationId();

        app.MapGroup("api/azurelibrary")
            .RequireCors(specificOrigins)
            .MapChangesHandler()
            .MapVariableGroupHandler()
            .MapProjectHandler()
            .MapSecretHandler();

        await ApplyDbMigrationsAsync(app);
        RegisterStartupReadiness(app);

        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseHttpsRedirection();
        app.UseCors(specificOrigins);
        app.UseAuthorization();

        return app;
    }

    private static async Task ApplyDbMigrationsAsync(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var operationDbContext = scope.ServiceProvider.GetRequiredService<OperationsDbContext>();
        await operationDbContext.Database.MigrateAsync();
    }

    private static void RegisterStartupReadiness(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        scope.ServiceProvider.GetRequiredService<StartupHealthCheck>().RegisterStartupReadiness();
    }
}
