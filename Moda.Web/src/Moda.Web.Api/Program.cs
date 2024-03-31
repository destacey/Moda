using System.Reflection;
using FluentValidation.AspNetCore;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using Moda.AppIntegration.Application;
using Moda.Common.Application;
using Moda.Common.Application.Interfaces;
using Moda.Goals.Application;
using Moda.Health;
using Moda.Infrastructure;
using Moda.Infrastructure.Common;
using Moda.Links;
using Moda.Organization.Application;
using Moda.Planning.Application;
using Moda.Web.Api.Configurations;
using Moda.Web.Api.Interfaces;
using Moda.Web.Api.Services;
using Moda.Work.Application;
using NodaTime.Serialization.SystemTextJson;
using Serilog;

TelemetryDebugWriter.IsTracingDisabled = true;

StaticLogger.EnsureInitialized();
Log.Information("Server Booting Up...");
try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.AddConfigurations();
    builder.Host.UseSerilog((context, config) =>
    {
        config.ReadFrom.Configuration(context.Configuration);
        config.Enrich.WithProperty("version", Environment.GetEnvironmentVariable("version") ?? Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "none-supplied");
    });
    if (builder.Environment.IsDevelopment())
    {
        Serilog.Debugging.SelfLog.Enable(Console.Error);
    }

    builder.Services.AddControllers()
        .AddJsonOptions(options => options.JsonSerializerOptions.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var problemDetails = new ValidationProblemDetails(context.ModelState)
            {
                Type = "https://developer.mozilla.org/en-US/docs/web/http/status/422",
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status422UnprocessableEntity,
                Detail = "See the errors property for details.",
                Instance = context.HttpContext.Request.Path
            };

            return new UnprocessableEntityObjectResult(problemDetails)
            {
                ContentTypes = { "application/problem+json" }
            };
        };
    });

    builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());
    builder.Services.AddFluentValidationAutoValidation();
    builder.Services.AddFluentValidationClientsideAdapters();

    builder.Services.AddCommonApplication();
    builder.Services.AddInfrastructure(builder.Configuration);
    builder.Services.AddOrganizationApplication();
    builder.Services.AddPlanningApplication();
    builder.Services.AddGoalsApplication();
    builder.Services.AddHealthApplication();
    builder.Services.AddLinksApplication();
    builder.Services.AddWorkApplication();
    builder.Services.AddAppIntegrationApplication();

    builder.Services.AddScoped<ICsvService, CsvService>();
    builder.Services.AddScoped<IJobManager, JobManager>();

    var app = builder.Build();

    await app.Services.InitializeDatabases();

    app.UseInfrastructure(builder.Configuration);
    app.MapEndpoints();
    app.MapGet("/startup", () => Results.Ok());
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
