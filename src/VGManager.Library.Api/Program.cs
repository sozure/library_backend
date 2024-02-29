namespace VGManager.Library.Api;

static partial class Program
{
    public static async Task Main(string[] args)
    {
        var specificOrigins = Constants.Cors.AllowSpecificOrigins;

        var builder = WebApplication.CreateBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole();

        ConfigureServices(builder, specificOrigins);

        var app = builder.Build();

        await ConfigureAsync(app, specificOrigins);
        await app.RunAsync();
    }
}


