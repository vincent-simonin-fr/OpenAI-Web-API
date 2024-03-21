using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using MemoryRecord = Microsoft.SemanticKernel.Memory.MemoryRecord;

namespace MagellanGPT.Infrastructure.OpenAI;

/// <summary>
/// ExtractContent from PDF https://github.com/microsoft/kernel-memory/blob/main/examples/205-dotnet-extract-text-from-docs/Program.cs
/// Memory & GetEmbedding https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs
/// && https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-planner-improvements-with-embeddings-and-semantic-memory/
/// https://devblogs.microsoft.com/semantic-kernel/elasticsearch-kernelmemory/
/// </summary>
public class AzureAiSearchService : IAzureAiSearchService
{
    private const string MemoryCollectionName = "SKMagellanGPT1";
    private readonly string _azureBlobStorageConnectionString;

    private readonly IKernelMemory _kernelMemory;

#pragma warning disable SKEXP0021
    private readonly AzureAISearchMemoryStore _azureAISearchMemoryStore;
    private readonly IOpenAIService _openAIService;

    public AzureAiSearchService(IConfiguration configuration, IOpenAIService openAIService)
    {
        var openAiEndpoint = configuration.GetSection("OpenAi:Endpoint").Value!;
        var openAiKey = SecretManager.GetInstance().OpenAiKey;
        var aiSearchEndpoint = configuration.GetSection("AiSearch:Endpoint").Value!;
        var aiSearchKey = SecretManager.GetInstance().AiSearchKey;
        _azureBlobStorageConnectionString = configuration.GetSection("AzureBlobStorage:ConnectionString").Value!;


        // https://github.com/microsoft/kernel-memory/blob/main/service/Core/Configuration/KernelMemoryConfig.cs
        _kernelMemory = new KernelMemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig
            {
                Endpoint = openAiEndpoint,
                APIKey = openAiKey,
                Deployment = "text-embedding-ada-002",
                APIType = AzureOpenAIConfig.APITypes.EmbeddingGeneration,
                MaxTokenTotal = 10000,
                Auth = AzureOpenAIConfig.AuthTypes.APIKey
            })
            .WithAzureAISearchMemoryDb(new AzureAISearchConfig
            {
                Endpoint = aiSearchEndpoint,
                APIKey = aiSearchKey,
                Auth = AzureAISearchConfig.AuthTypes.APIKey,
                
            })
            .WithSearchClientConfig(new SearchClientConfig
            {
                
            })
            .WithOpenAITextGeneration(new OpenAIConfig
            {
                APIKey = openAiKey,
                EmbeddingModel = "text-embedding-ada-002",
                TextModel = "ChatGPT35Turbo",
                EmbeddingModelMaxTokenTotal = 10000,
                TextModelMaxTokenTotal = 16000
            })
            .WithAzureBlobsStorage(new AzureBlobsConfig
            {
                ConnectionString = $"DefaultEndpointsProtocol=https;{_azureBlobStorageConnectionString}",
                Container = "documents",
                Auth = AzureBlobsConfig.AuthTypes.ConnectionString
            })
            .WithCustomTextPartitioningOptions(new TextPartitioningOptions
            {
                MaxTokensPerLine = 60,
                MaxTokensPerParagraph = 150,
                OverlappingTokens = 20
            })
            .Build<MemoryServerless>();

        _openAIService = openAIService;
        _azureAISearchMemoryStore = new AzureAISearchMemoryStore(aiSearchEndpoint, aiSearchKey);
    }

    public async Task<int> StoreAsync(Dictionary<string, string> documents)
    {
        int tokenCost = 0;
        foreach (var doc in documents)
        {
            tokenCost += StoreMemoryRecordAsync(doc.Key, doc.Value).Result;
        }

        return tokenCost;
    }

    /// <summary>
    /// WIP - Improve query parameter and return
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<SearchResult> SearchMemoryAsync(string query)
    {
        var searchResult = await _kernelMemory.SearchAsync(query: query, index: "document", limit: 10, minRelevance: 0.5);

        return searchResult;
    }

    /// <summary>
    /// Memorization
    /// https://github.com/Azure-Samples/azure-search-sample-data
    /// </summary>
    /// <param name="documentKey"></param>
    /// <param name="documentValue"></param>
    /// <returns>Returns token cost of embeddings</returns>
    private async Task<int> StoreMemoryRecordAsync(string documentKey, string documentValue)
    {
        // TODO L'utilisation de kernel memory est à améliorer
        // L'import de document fonctionne correctement mais les erreur ne sont pas géré
        // Le requêtage ne fonctionne pas, cela est proprablement du à la configuration de la pipeline
        var documentId = await _kernelMemory.ImportDocumentAsync(documentKey, index: "document");

        // var result = await _kernelMemory.AskAsync("Cuisson", index:"document", minRelevance: 0.7);

        var embeddingsDict = await _openAIService.GetEmbeddings(documentValue);
        var totalTokens = 0;
        var index = 0;
        foreach (var embedding in embeddingsDict)
        {
#pragma warning disable SKEXP0001
            var memoryRecordMetadata = new MemoryRecordMetadata(true, $"{documentKey}-{index}", embedding.Value.Text, embedding.Value.Text.Substring(0, 100), string.Empty, string.Empty);

            var memoryRecord = new MemoryRecord(memoryRecordMetadata, embedding.Value.Embeddings.Data[0].Embedding, documentKey, DateTimeOffset.UtcNow);

            await _azureAISearchMemoryStore.UpsertAsync(MemoryCollectionName, memoryRecord);

            totalTokens += embedding.Value.Embeddings.Usage.TotalTokens;

            index++;
        }
        return totalTokens;
    }
}

