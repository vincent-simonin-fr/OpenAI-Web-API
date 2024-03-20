using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MagellanGPT.Infrastructure.KeyVault;

public class AzureKeyvaultService : IAzureKeyvaultService
{
    private readonly SecretClient _secretClient;

    public AzureKeyvaultService(IConfiguration configuration)
    {
        var keyvaultUri = configuration["KeyVault:Uri"];
        var tenantId = configuration["KeyVault:TenantId"];
        var clientId = configuration["KeyVault:ClientId"];
        var clientSecret = configuration["KeyVault:ClientSecret"];
        var credentials = new ClientSecretCredential(tenantId, clientId, clientSecret);
        _secretClient = new SecretClient(new Uri(keyvaultUri), credentials);
    }

    public string GetSecret(string secretName)
    {
        KeyVaultSecret secret = _secretClient.GetSecret(secretName);
        return secret.Value;
    }
}

