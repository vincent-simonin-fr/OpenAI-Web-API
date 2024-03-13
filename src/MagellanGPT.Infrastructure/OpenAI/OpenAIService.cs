using Azure;
using Azure.AI.OpenAI;
using MagellanGPT.Application.Common.Interfaces;
using MagellanGPT.Domain.Entities;
using MagellanGPT.Infrastructure.KeyVault;
using Microsoft.Extensions.Configuration;

namespace MagellanGPT.Infrastructure.OpenAI;

/// <summary>
/// Use Semantic Kernel
/// https://devblogs.microsoft.com/dotnet/demystifying-retrieval-augmented-generation-with-dotnet/
/// </summary>
public class OpenAIService : IOpenAIService
{
    private readonly OpenAIClient _client;
    private string _deploymentName;

    public OpenAIService(IConfiguration configuration)
    {
        _client = new OpenAIClient(
          new Uri(configuration.GetSection("OpenAi:Endpoint").Value!),
          new AzureKeyCredential(SecretManager.GetInstance().OpenAiKey));

        _deploymentName = configuration.GetSection("OpenAi:DefaultDeploymentName").Value!;
    }

    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemand(string question, string? deploymentName = null)
    {
        _deploymentName = deploymentName is not null ? deploymentName : _deploymentName;

        StreamingResponse <StreamingChatCompletionsUpdate> responseStreamed = await _client.GetChatCompletionsStreamingAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
          {
              new ChatRequestSystemMessage(@"You are an AI assistant that helps people find information.
                    Pour information:
                    la date d'anniversaire de victor levy est le 7 octobre"),
              new ChatRequestUserMessage(@"quand est l'anniversaire de victor levy ?"),
                new ChatRequestAssistantMessage(@"L'anniversaire de Victor Levy est le 7 octobre."),
                new ChatRequestUserMessage(@"et jules perrodon ?"),
                new ChatRequestAssistantMessage(@"Je suis désolé, mais je n'ai pas d'informations sur la date d'anniversaire de Jules Perrodon."),
                new ChatRequestUserMessage(question),
          },
            Temperature = 1,
            MaxTokens = 800,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        return responseStreamed.EnumerateValues();
    }


    public async Task<IAsyncEnumerable<StreamingChatCompletionsUpdate>> ProcessDemandWithRag(string question, string document, string? deploymentName = null)
    {
        _deploymentName = deploymentName is not null ? deploymentName : _deploymentName;

        // Prompt Chaining https://www.promptingguide.ai/fr/techniques/prompt_chaining
        StreamingResponse<StreamingChatCompletionsUpdate> responseStreamed = await _client.GetChatCompletionsStreamingAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
            {
                new ChatRequestSystemMessage($"Tu es un expert quelque soit le domaine." +
                $"Ta tâche est d'aider à répondre à une question en utilisant un document. " +
                $"La première étape est d'extraire des informations pertinentes du document, délimité par ###" +
                $". Génère une réponse. " +
                $"### {document} ###"),
                new ChatRequestUserMessage(question),
            },
            Temperature = 1,
            MaxTokens = 800,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        return responseStreamed.EnumerateValues();
    }

    // TODO: Remove if not used
    public async Task<(string Text, int TotalTokens, int RequestTokens, int ResponseTokens)> ProcessDemandSynchronously(Conversation conversation)
    {
        List<ChatRequestMessage> messages = new List<ChatRequestMessage>() {
                new ChatRequestSystemMessage(@"Tu es un assistant IA expert."),
            };

        conversation.Dialogs.ForEach(dialog =>
        {
            if(dialog.Answer is not null)
            {
                messages.Add(new ChatRequestUserMessage(dialog.Question));
                messages.Add(new ChatRequestUserMessage(dialog.Answer));
            }
        });

        var question = conversation.Dialogs[^1].Question;

        messages.Add(new ChatRequestUserMessage(question));

        ChatCompletions responseWithoutStream = await _client.GetChatCompletionsAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
            {
                new ChatRequestSystemMessage(@"Tu es un assistant IA expert."),
                new ChatRequestUserMessage(question),
            },
            Temperature = 0.5f,
            MaxTokens = 800,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        var completionTokens = responseWithoutStream.Usage.CompletionTokens;
        var requestTokens = responseWithoutStream.Usage.PromptTokens;
        var totalTokens = responseWithoutStream.Usage.TotalTokens;

        return (responseWithoutStream.Choices[0].Message.Content, totalTokens, requestTokens, completionTokens);
    }

    public async Task<(string Text, int TotalTokens, int RequestTokens, int ResponseTokens)> ProcessDemandWithRagSynchronously(string question, string document, string? deploymentName = null)
    {
        _deploymentName = deploymentName is not null ? deploymentName : _deploymentName;

        // Prompt Chaining https://www.promptingguide.ai/fr/techniques/prompt_chaining
        ChatCompletions response = await _client.GetChatCompletionsAsync(
        new ChatCompletionsOptions()
        {
            DeploymentName = _deploymentName,
            Messages =
            {
                new ChatRequestSystemMessage($"Tu es un expert quelque soit le domaine." +
                $"Ta tâche est d'aider à répondre à une question en utilisant un document et tes connaissances. " +
                $"La première étape est d'extraire des informations pertinentes du document, délimité par ###" +
                $". Génère une réponse. " +
                $"### {document} ###"),
                new ChatRequestUserMessage(question),
            },
            Temperature = 1,
            MaxTokens = 800,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        });

        var completionTokens = response.Usage.CompletionTokens;
        var requestTokens = response.Usage.PromptTokens;
        var totalTokens = response.Usage.TotalTokens;

        return (response.Choices[0].Message.Content, totalTokens, requestTokens, completionTokens);
    }

    public async Task<(ReadOnlyMemory<float> EmbeddingArray, int TotalTokens)> GetEmbeddingsAsync(string document)
    {
        EmbeddingsOptions embeddingOptions = new()
        {
            DeploymentName = "text-embedding-ada-002",
            Input = { document },
        };

        var returnValue = await _client.GetEmbeddingsAsync(embeddingOptions);

        var totalTokens = returnValue.Value.Usage.TotalTokens;
        var embeddingArray = returnValue.Value.Data[0].Embedding;

        foreach (float item in returnValue.Value.Data[0].Embedding.ToArray())
        {
            Console.WriteLine(item);
        }

        return (embeddingArray, totalTokens);
    }
}

