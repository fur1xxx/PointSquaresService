using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Core;

try
{
    var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = args, ContentRootPath = AppContext.BaseDirectory });
    builder.Services.AddPersistence(builder.Configuration);

    using var host = builder.Build();
    await using var scope = host.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<PointSquaresDbContext>().Database.MigrateAsync();

    Console.WriteLine("Migrations applied.");
    return 0;
}
catch (Exception exception) when (exception is not HostAbortedException)
{
    await Console.Error.WriteLineAsync($"Migrations failed: {exception}");
    return 1;
}
