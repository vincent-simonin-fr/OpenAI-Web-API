using Azure;
using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.KeyVault;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;

namespace MagellanGPT.Infrastructure.OpenAI;

/// <summary>
/// ExtractContent from PDF https://github.com/microsoft/kernel-memory/blob/main/examples/205-dotnet-extract-text-from-docs/Program.cs
/// Memory & GetEmbedding https://github.com/microsoft/semantic-kernel/blob/main/dotnet/samples/KernelSyntaxExamples/Example14_SemanticMemory.cs
/// && https://devblogs.microsoft.com/semantic-kernel/semantic-kernel-planner-improvements-with-embeddings-and-semantic-memory/
/// https://devblogs.microsoft.com/semantic-kernel/elasticsearch-kernelmemory/
/// </summary>
public class AzureAiSearchService : IAzureAiSearchService
{
    private const string MemoryCollectionName = "SKOrganization";

#pragma warning disable SKEXP0003
    private readonly ISemanticTextMemory _memory;

    private readonly OpenAIClient _openAIClient;
#pragma warning disable SKEXP0021
    private readonly AzureAISearchMemoryStore _azureAISearchMemoryStore;

    public AzureAiSearchService(IConfiguration configuration)
    {
        var openAiEndpoint = configuration.GetSection("OpenAi:Endpoint").Value!;
        var openAiKey = SecretManager.GetInstance().OpenAiKey;
        var aiSearchEndpoint = configuration.GetSection("AiSearch:Endpoint").Value!;
        var aiSearchKey = SecretManager.GetInstance().AiSearchKey;

#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0021
        _memory = new MemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration("text-embedding-ada-002", openAiEndpoint, openAiKey)
            .WithMemoryStore(new AzureAISearchMemoryStore(aiSearchEndpoint, aiSearchKey))
            .Build();

        AzureKeyCredential credentials = new(openAiKey);
        _openAIClient = new(new Uri(openAiEndpoint), credentials);

        _azureAISearchMemoryStore = new AzureAISearchMemoryStore(aiSearchEndpoint, aiSearchKey);
    }

    public async Task StoreAsync(Dictionary<string, string> documents)
    {

        foreach (var doc in documents)
        {
            //var docId = await _memory.SaveReferenceAsync(
            //    collection: MemoryCollectionName,
            //    externalSourceName: "Organization",
            //    externalId: doc.Key,
            //    description: doc.Value,
            //    text: doc.Value);

            StoreMemoryRecordAsync(doc.Key, doc.Value);
        }
    }

    /// <summary>
    /// WIP - Improve query parameter and return
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task SearchMemoryAsync(string query)
    {
        var memoryResults = _memory.SearchAsync(MemoryCollectionName, query, limit: 2, minRelevanceScore: 0.5);

        int i = 0;
        await foreach (MemoryQueryResult memoryResult in memoryResults)
        {
            Console.WriteLine($"Result {++i}:");
            Console.WriteLine("  URL:     : " + memoryResult.Metadata.Id);
            Console.WriteLine("  Title    : " + memoryResult.Metadata.Description);
            Console.WriteLine("  Relevance: " + memoryResult.Relevance);
            Console.WriteLine();
        }

        Console.WriteLine("----------------------");
    }

    public async Task<ReadOnlyMemory<float>> GetEmbeddingsAsync(string document)
    {
        EmbeddingsOptions embeddingOptions = new()
        {
            DeploymentName = "text-embedding-ada-002",
            Input = { document },
        };

        var returnValue = await _openAIClient.GetEmbeddingsAsync(embeddingOptions);

        var requestTokens = returnValue.Value.Usage.PromptTokens;
        var totalTokens = returnValue.Value.Usage.TotalTokens;
        var embeddingArray = returnValue.Value.Data[0].Embedding;

        foreach (float item in returnValue.Value.Data[0].Embedding.ToArray())
        {
            Console.WriteLine(item);
        }

        return embeddingArray;
    }

    public async void StoreMemoryRecordAsync(string documentKey, string documentValue)
    {
        var embedding = await GetEmbeddingsAsync(documentValue);

        var MemoryRecordMetadata = new MemoryRecordMetadata(true, Guid.NewGuid().ToString(), documentKey, documentValue, string.Empty, string.Empty);

        var memoryRecord = new MemoryRecord(MemoryRecordMetadata, embedding, documentKey, DateTimeOffset.UtcNow);

        await _azureAISearchMemoryStore.UpsertAsync(MemoryCollectionName, memoryRecord);
    }
}

