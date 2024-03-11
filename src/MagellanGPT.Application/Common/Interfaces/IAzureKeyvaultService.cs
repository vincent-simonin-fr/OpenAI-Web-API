namespace MagellanGPT.Application.Common.Interfaces;

public interface IAzureKeyvaultService
{
    string GetSecret(string secretName);
}

