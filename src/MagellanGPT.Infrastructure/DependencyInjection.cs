using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.KeyVault;
using MagellanGPT.Infrastructure.OpenAI;
using MagellanGPT.Infrastructure.Persistence;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var accountEndPoint = configuration.GetSection("CosmosDb:Endpoint").Value!;
        var accountKey = configuration.GetSection("CosmosDb:Key").Value!;
        var dbName = configuration.GetSection("CosmosDb:DbName").Value!;

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseCosmos(accountEndPoint, accountKey, dbName, options =>
            {
                // https://github.com/dotnet/EntityFramework.Docs/blob/main/samples/core/Cosmos/ModelBuilding
                options.ConnectionMode(ConnectionMode.Gateway);
            }
            ));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddScoped<ApplicationDbContextInitialiser>();

        services.AddScoped<IOpenAIService, OpenAIService>();

        services.AddScoped<IAzureAiSearchService, AzureAiSearchService>();

        services.AddScoped<IAzureKeyvaultService, AzureKeyvaultService>();

        return services;
    }
}

