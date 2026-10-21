using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Core.Repositories;

namespace Persistence.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("PointSquares") ?? throw new InvalidOperationException("Connection string 'PointSquares' is not set.");
        services.AddDbContext<PointSquaresDbContext>(options =>
            options
                .UseMySQL(connectionString, o => o.MaxBatchSize(1_000))
                .ConfigureWarnings(w =>
                    w.Log(
                        (RelationalEventId.CommandError, LogLevel.Warning),
                        (CoreEventId.QueryIterationFailed, LogLevel.Warning),
                        (CoreEventId.SaveChangesFailed, LogLevel.Warning)
                    )
                )
        );
        services.AddScoped<UnitOfWork>();
        services.AddScoped<PointsCollectionRepository>();
        services.AddScoped<PointsRepository>();
        return services;
    }
}
