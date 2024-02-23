using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var accountEndPoint = configuration.GetSection("CosmosDb:EndPoint").Value ?? throw new ArgumentException("CosmosDb:EndPoint not found");
        var accountKey = configuration.GetSection("CosmosDb:Key").Value ?? throw new ArgumentException("CosmosDb:Key not found");
        var dbName = configuration.GetSection("CosmosDb:DbName").Value ?? throw new ArgumentException("CosmosDb:DbName not found");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseCosmos(accountEndPoint, accountKey, dbName));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        return services;
    }
}

