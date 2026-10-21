using Microsoft.Extensions.DependencyInjection;

namespace Domain.Handlers;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddHandlers(this IServiceCollection services)
    {
        services.AddScoped<SquaresCounter>();
        services.AddScoped<AddPointHandler>();
        services.AddScoped<RemovePointHandler>();
        services.AddScoped<ImportPointsHandler>();
        return services;
    }
}
