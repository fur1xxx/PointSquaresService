using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Persistence.Core.Repositories;

namespace Persistence.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("PointSquares") ?? throw new InvalidOperationException("Connection string 'PointSquares' is not set.");
        services.AddDbContext<PointSquaresDbContext>(options => options.UseMySQL(connectionString, o => o.MaxBatchSize(1_000)));
        services.AddScoped<PointsCollectionRepository>();
        return services;
    }
}
