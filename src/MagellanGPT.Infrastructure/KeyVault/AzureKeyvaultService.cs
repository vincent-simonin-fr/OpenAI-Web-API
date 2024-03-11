using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using MagellanGPT.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace MagellanGPT.Infrastructure.KeyVault;

public class AzureKeyvaultService : IAzureKeyvaultService
{
    private readonly SecretClient _secretClient;
    // private readonly IConfiguration _configuration;

    public AzureKeyvaultService(IConfiguration configuration)
    {
        // _configuration = configuration;
        var keyvaultUri = configuration["KeyVault:Uri"]!;
        var tenantId = configuration["KeyVault:TenantId"];
        var clientId = configuration["KeyVault:ClientId"];
        var secret = configuration["KeyVault:ClientSecret"];
        var credentials = new ClientSecretCredential(tenantId: tenantId, clientId: clientId, clientSecret: secret);
        _secretClient = new SecretClient(new Uri(keyvaultUri), credentials);
    }

    public string GetSecret(string secretName)
    {
        KeyVaultSecret secret = _secretClient.GetSecret(secretName);
        return secret.Value;
    }
}

