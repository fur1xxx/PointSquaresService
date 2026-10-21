using System.Globalization;
using Persistence.Core;
using PointSquaresService.ErrorHandling;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var serviceUrlPrefix = "point-squares";
var swaggerUrlPrefix = $"{serviceUrlPrefix}/swagger";

builder.Services.AddSerilog(
    (services, logger) =>
        logger
            .ReadFrom.Configuration(builder.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console(formatProvider: CultureInfo.InvariantCulture)
);

builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddHealthChecks().AddDbContextCheck<PointSquaresDbContext>();
builder.Services.AddOpenApi();
builder.Services.AddExceptionHandler<ApiExceptionHandler>();
builder.Services.AddProblemDetails();
builder.Services.Configure<RouteHandlerOptions>(options => options.ThrowOnBadRequest = true);

var app = builder.Build();

app.UseHealthChecks($"/{serviceUrlPrefix}/v1/health-check");
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseRouting();

if (!app.Environment.IsProduction())
{
    app.MapOpenApi($"/{swaggerUrlPrefix}/{{documentName}}/openapi.json");
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint($"/{swaggerUrlPrefix}/v1/openapi.json", "v1");
        options.RoutePrefix = swaggerUrlPrefix;
    });
}

await app.RunAsync();
