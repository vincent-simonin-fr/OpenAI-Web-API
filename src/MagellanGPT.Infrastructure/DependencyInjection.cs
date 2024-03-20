using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.Constant;
using MagellanGPT.Infrastructure.Identity;
using MagellanGPT.Infrastructure.KeyVault;
using MagellanGPT.Infrastructure.OpenAI;
using MagellanGPT.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<UserManager<ApplicationUser>>();
        services.AddTransient<IIdentityService, IdentityService>();

        services
            .AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IAzureKeyvaultService, AzureKeyvaultService>();

        InitSecretManager(configuration);

        var accountEndPoint = configuration.GetSection("CosmosDb:EndPoint").Value!;
        var accountKey = SecretManager.GetInstance().CosmosDbKey;
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

        return services;
    }

    private static void InitSecretManager(IConfiguration configuration)
    {
        var azureKeyvaultService = new AzureKeyvaultService(configuration);

        SecretManager secretManager = SecretManager.GetInstance();

        secretManager.OpenAiKey = azureKeyvaultService.GetSecret(ConfigurationConstants.DevOpenAiKey);
        secretManager.AiSearchKey = azureKeyvaultService.GetSecret(ConfigurationConstants.DevAiSearchKey);
        secretManager.CosmosDbKey = azureKeyvaultService.GetSecret(ConfigurationConstants.DevCosmosDbKey);
    }
}

