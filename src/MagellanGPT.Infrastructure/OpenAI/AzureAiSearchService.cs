using Azure;
using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
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
    private const string MemoryCollectionName = "SKGitHub";
#pragma warning disable SKEXP0003
    private readonly ISemanticTextMemory _memory;
    private readonly string _openAiEndpoint;
    private readonly string _openAiKey;

    public AzureAiSearchService(IConfiguration configuration)
    {
        _openAiEndpoint = configuration.GetSection("OpenAi:Endpoint").Value!;
        _openAiKey = configuration.GetSection("OpenAi:Key").Value!;
        var aiSearchEndpoint = configuration.GetSection("AiSearch:Endpoint").Value!;
        var aiSearchKey = configuration.GetSection("AiSearch:Key").Value!;

#pragma warning disable SKEXP0003
#pragma warning disable SKEXP0011
#pragma warning disable SKEXP0021
        _memory = new MemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration("text-embedding-ada-002", _openAiEndpoint, _openAiKey)
            .WithMemoryStore(new AzureAISearchMemoryStore(aiSearchEndpoint, aiSearchKey))
            .Build();
    }

    public async Task StoreAsync(Dictionary<string, string> documents)
    {

        foreach (var doc in documents)
        {
            var docId = await _memory.SaveReferenceAsync(
                collection: MemoryCollectionName,
                externalSourceName: "Organization",
                externalId: doc.Key,
                description: doc.Value,
                text: doc.Value);
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

    public async Task<float[]> GetEmbeddings(string document)
    {
        EmbeddingsOptions embeddingOptions = new()
        {
            DeploymentName = "text-embedding-ada-002",
            Input = { document },
        };

        AzureKeyCredential credentials = new(_openAiKey);
        OpenAIClient openAIClient = new(new Uri(_openAiEndpoint), credentials);

        var returnValue = await openAIClient.GetEmbeddingsAsync(embeddingOptions);

        var requestTokens = returnValue.Value.Usage.PromptTokens;
        var totalTokens = returnValue.Value.Usage.TotalTokens;
        var embeddingArray = returnValue.Value.Data[0].Embedding.ToArray();

        foreach (float item in returnValue.Value.Data[0].Embedding.ToArray())
        {
            Console.WriteLine(item);
        }

        return embeddingArray;
    }
}

