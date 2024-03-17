using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Infrastructure.KeyVault;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel.Connectors.AzureAISearch;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Microsoft.SemanticKernel.Memory;
using Microsoft.SemanticKernel.Text;
using static System.Net.Mime.MediaTypeNames;

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

#pragma warning disable SKEXP0021
    private readonly AzureAISearchMemoryStore _azureAISearchMemoryStore;
    private readonly IOpenAIService _openAIService;

    public AzureAiSearchService(IConfiguration configuration, IOpenAIService openAIService)
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

        _openAIService = openAIService;
        _azureAISearchMemoryStore = new AzureAISearchMemoryStore(aiSearchEndpoint, aiSearchKey);
    }

    public async Task<int> StoreAsync(Dictionary<string, string> documents)
    {
        int tokenCost = 0;
        foreach (var doc in documents)
        {
            //var docId = await _memory.SaveReferenceAsync(
            //    collection: MemoryCollectionName,
            //    externalSourceName: "Organization",
            //    externalId: doc.Key,
            //    description: doc.Value,
            //    text: doc.Value);

            tokenCost += StoreMemoryRecordAsync(doc.Key, doc.Value).Result;
        }

        return tokenCost;
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

    /// <summary>
    /// Returns token cost of embeddings
    /// </summary>
    /// <param name="documentKey"></param>
    /// <param name="documentValue"></param>
    /// <returns></returns>
    private async Task<int> StoreMemoryRecordAsync(string documentKey, string documentValue)
    {
        (ReadOnlyMemory<float> EmbeddingArray, int TotalTokens) response = await _openAIService.GetEmbeddingsAsync(documentValue);

#pragma warning disable SKEXP0055
        var lines = TextChunker.SplitPlainTextLines(documentValue, 40);
        var paragraphs = TextChunker.SplitPlainTextParagraphs(lines, 120);

        var memoryRecordMetadata = new MemoryRecordMetadata(true, documentKey, documentKey, documentValue, string.Empty, string.Empty);

        var memoryRecord = new MemoryRecord(memoryRecordMetadata, response.EmbeddingArray, documentKey, DateTimeOffset.UtcNow);

        await _azureAISearchMemoryStore.UpsertAsync(MemoryCollectionName, memoryRecord);

        return response.TotalTokens;
    }
}

