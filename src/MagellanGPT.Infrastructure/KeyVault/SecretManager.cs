namespace MagellanGPT.Infrastructure.KeyVault;

public class SecretManager
{
    private static SecretManager _instance;

    public string OpenAiKey { get; set;}
    public string AiSearchKey { get; set; }
    public string CosmosDbKey { get; set; }

    public static SecretManager GetInstance()
    {
        if (_instance == null)
        {
            _instance = new SecretManager();
        }
        return _instance;
    }
}

