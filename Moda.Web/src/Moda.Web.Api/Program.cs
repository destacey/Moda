using Moda.Common.Application;
using Moda.Infrastructure;
using Moda.Infrastructure.Common;
using Moda.Organization.Application;
using Moda.Web.Api.Configurations;
using Serilog;
using NodaTime.Serialization.SystemTextJson;
using NodaTime;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddConfigurations();
    builder.Host.UseSerilog((_, config) =>
    {
        config.WriteTo.Console()
            .ReadFrom.Configuration(builder.Configuration);
    });

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

    //// removing for now since auto validation is not asynchronous.  https://docs.fluentvalidation.net/en/latest/aspnet.html
    //builder.Services.AddFluentValidationAutoValidation();
    //builder.Services.AddFluentValidationClientsideAdapters();

    builder.Services.AddCommonApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddOrganizationApplication();

    var app = builder.Build();

    await app.Services.InitializeDatabases();

    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();
    app.Run();
}
catch (Exception ex) when (!ex.GetType().Name.Equals("HostAbortedException", StringComparison.Ordinal))
{
    StaticLogger.EnsureInitialized();
    Log.Fatal(ex, "Unhandled exception");
}
finally
{
    StaticLogger.EnsureInitialized();
    Log.Information("Server Shutting down...");
    Log.CloseAndFlush();
}
