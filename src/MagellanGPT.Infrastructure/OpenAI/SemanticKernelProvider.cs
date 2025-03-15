using MagellanGPT.Infrastructure.KeyVault;
using Microsoft.SemanticKernel;
using Microsoft.Extensions.Configuration;
using Microsoft.KernelMemory;
using Microsoft.KernelMemory.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel.ChatCompletion;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Domain.Entities;
using Azure;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace MagellanGPT.Infrastructure.OpenAI;

/// <summary>
/// Extension methods for registering Semantic Kernel related services.
/// </summary>
public class SemanticKernelProvider : ISemanticKernelProvider
{
    private readonly MemoryServerless _kernelMemory;
    private readonly Kernel _kernel;
    private readonly OpenAIClient _openAiClient;

    private const string TextEmbeddingModel = "text-embedding-ada-002";
    private const string IndexName = "doc";
    private const float MinRelevance = 0.75f;
    private const int LimitNumberDocument = 4;

    public SemanticKernelProvider(IConfiguration configuration, Kernel kernel)
    {
        var openAiEndpoint = configuration.GetSection("OpenAi:Endpoint").Value!;
        var openAiKey = SecretManager.GetInstance().OpenAiKey;
        var aiSearchEndpoint = configuration.GetSection("AiSearch:Endpoint").Value!;
        var aiSearchKey = SecretManager.GetInstance().AiSearchKey;
        var azureBlobStorageConnectionString = configuration.GetSection("AzureBlobStorage:ConnectionString").Value!;

        // https://github.com/microsoft/kernel-memory/blob/main/service/Core/Configuration/KernelMemoryConfig.cs
        _kernelMemory = new KernelMemoryBuilder()
            .WithAzureOpenAITextEmbeddingGeneration(new AzureOpenAIConfig
            {
                Endpoint = openAiEndpoint,
                APIKey = openAiKey,
                Deployment = TextEmbeddingModel,
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
                // TODO : Improve custm config
                Temperature = 1,
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
                ConnectionString = $"DefaultEndpointsProtocol=https;{azureBlobStorageConnectionString}",
                Container = "documents",
                Auth = AzureBlobsConfig.AuthTypes.ConnectionString
            })
            .WithCustomTextPartitioningOptions(new TextPartitioningOptions
            {
                MaxTokensPerLine = 40,
                MaxTokensPerParagraph = 500,
                OverlappingTokens = 200
            })
            .Build<MemoryServerless>();

        _kernel = kernel;

        _openAiClient = new OpenAIClient(
          new Uri(configuration.GetSection("OpenAi:Endpoint").Value!),
          new AzureKeyCredential(SecretManager.GetInstance().OpenAiKey));
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<ChatMessageContent> ProcessUserRequest(Conversation conversation)
    {
        _kernel.Culture = System.Globalization.CultureInfo.InvariantCulture;
        var chat = _kernel.Services.GetRequiredKeyedService<IChatCompletionService>("ChatCompletion");

        var query = conversation.Dialogs[^1].Question;

        var searchResult = await _kernelMemory.SearchAsync(query: query, index: IndexName, limit: 4, minRelevance: 0.75);

        var history = LoadChatHistory(conversation);

        // Bof bof
        searchResult.Results.ForEach(async result =>
        {
            var prompt = $"Analyze the following text extract " +
                $"and generate a 3-word shot for me : ###{String.Join("\n\n", result.Partitions.Select(p => p.Text))}###";

            List<ChatRequestMessage> messages = new List<ChatRequestMessage>() {
                new ChatRequestSystemMessage(conversation.SystemPrompt),
                new ChatRequestUserMessage(prompt)
            };

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                DeploymentName = "ChatGPT35Turbo",
                Temperature = (float)0.7,
                MaxTokens = 800,
                NucleusSamplingFactor = (float)0.95,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
            };

            messages.ForEach(chatCompletionsOptions.Messages.Add);

            var chatMessage = await _openAiClient.GetChatCompletionsAsync(chatCompletionsOptions);

            history.AddUserMessage(prompt);
            history.AddAssistantMessage(chatMessage.Value.Choices[0].Message.Content);
        });


        var chatMessage = await chat.GetChatMessageContentAsync(history);

        var yo = chatMessage.InnerContent!.ToString();
        Console.WriteLine(chatMessage.Encoding);
        conversation.Dialogs[^1].Answer = chatMessage.InnerContent.ToString();

        //TextGenerationOptions options = new()
        //{
        //    Temperature = 0.8,
        //    MaxTokens = 800
        //};
        //var test = _kernelMemory.Orchestrator.GetTextGenerator().GenerateTextAsync(query, options).GetAsyncEnumerator();
        //StringBuilder sb = new();
        //foreach(var yo in test.Current)
        //{
        //    sb.Append(yo.ToString());
        //}

        return chatMessage;
    }

    private static ChatHistory LoadChatHistory(Conversation conversation)
    {
        var chatHistory = new ChatHistory();

        if(conversation.SystemPrompt is not null) chatHistory.AddSystemMessage(conversation.SystemPrompt);
        conversation.Dialogs!.ForEach(dialog =>
        {
            chatHistory.AddUserMessage(dialog.Question);

            if (dialog.Answer is not null)
            {
                chatHistory.AddAssistantMessage(dialog.Answer);
            }
        });

        return chatHistory;
    }

    /// <summary>
    /// https://github.com/microsoft/kernel-memory/blob/main/service/Core/Configuration/KernelMemoryConfig.cs
    /// https://github.com/Azure-Samples/azure-search-openai-demo-csharp
    /// </summary>
    /// <param name="filePathList"></param>
    /// <returns></returns>
    public async Task StoreDocumentAsync(List<string> filePathList)
    {
        foreach (var filePath in filePathList)
        {
            var isReady = false;

            // TODO improve management of the import by get the documentId
            // and use it with _kernelMemory.GetDocumentStatusAsync & _kernelMemory.IsDocumentReadyAsync
            var documentId = await _kernelMemory.ImportDocumentAsync(filePath: filePath, index: IndexName);

            while (!isReady)
            {
                isReady = await _kernelMemory.IsDocumentReadyAsync(documentId, index: IndexName);
            }
        }
    }

    /// <summary>
    /// WIP - Improve query parameter and return
    /// </summary>
    /// <param name="query"></param>
    /// <returns></returns>
    public async Task<SearchResult> SearchCitationAsync(string query)
    {
        // Generate the embedding for the query  
        //var queryEmbeddings = await GenerateEmbeddings(query);

        //// https://github.com/Azure/azure-search-vector-samples/blob/main/demo-dotnet/DotNetVectorDemo/Program.cs
        //var searchOptions = new SearchOptions
        //{
        //    VectorSearch = new()
        //    {
        //        Queries = { new VectorizedQuery(queryEmbeddings.ToArray()) { KNearestNeighborsCount = 3, Fields = { "contentVector" } } }
        //    },
        //    Size = k,
        //    Select = { "title", "content", "category" },
        //};

        var searchResult = await _kernelMemory.SearchAsync(query: query, index: IndexName, limit: LimitNumberDocument, minRelevance: MinRelevance);

        return searchResult;
    }

    //private async Task<ReadOnlyMemory<float>> GenerateEmbeddings(string text)
    //{
    //    var response = await _openAiClient.GetEmbeddingsAsync(new EmbeddingsOptions(TextEmbeddingModel, new List<string> { text }));
    //    return response.Value.Data[0].Embedding;
    //}
}
